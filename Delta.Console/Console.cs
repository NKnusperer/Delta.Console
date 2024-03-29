﻿using System.Collections.Generic;
using Delta.Engine;
using Delta.Utilities.Datatypes;
using Delta.Rendering.Basics.Materials;
using Delta.Rendering.Basics.Fonts;
using Delta.InputSystem;
using Delta.Utilities.Datatypes.Advanced;
using System.Reflection;
using System.Linq;

namespace Delta.Console
{
    /// <summary>
    /// Draw the console overlay and handle text in/output
    /// </summary>
    class Console
    {
        #region Private
        /// <summary>
        /// Holds an instance of the CommandManager class
        /// </summary>
        private CommandManager cmdManager;
        /// <summary>
        /// Save if the console should be rendered or not
        /// </summary>
        private bool showConsole;
        /// <summary>
        /// The dimension of the console
        /// </summary>
        private Rectangle consoleDimension;
        /// <summary>
        /// The space between the screenspace and the console
        /// </summary>
        private Margin consoleMargin;
        /// <summary>
        /// The console color or material
        /// </summary>
        private Material2DColored consoleBackground;
        /// <summary>
        /// The font used for the console
        /// </summary>
        private Font consoleFont;
        /// <summary>
        /// A list of previous typed commands and output to render
        /// </summary>
        private List<string> history;
        /// <summary>
        /// The max. number of lines that will be displayed befor the oldest entry will be deleted
        /// </summary>
        private int maxNumberOfLines;
        /// <summary>
        /// A stacklist of previous typed commands to step for-/backward
        /// </summary>
        private Stack<string> cmdHistory;
        /// <summary>
        /// Helper to save what's the current index of the cmdHistory we display
        /// </summary>
        private int cmdHistoryPosition;
        /// <summary>
        /// A list containing commands that match the current input
        /// </summary>
        private List<string> autoCompletionCache;
        /// <summary>
        /// Quadratic space of the consoleFont
        /// </summary>
        private float fontHeight;
        /// <summary>
        /// Save the current typed text and return a cleared version
        /// </summary>
        private string _inputText;
        private string inputText
        {
            get
            {
                if (_inputText.Length > 0)
                {
                    // Remove the ending underscore
                    _inputText.Remove(_inputText.Length - 1);
                }
                // Remove the newline sequence
                return _inputText.Replace("\n", "");
            }
            set { _inputText = value; }
        }
        /// <summary>
        /// Save the last typed text. Needed for caching
        /// </summary>
        private string lastInputText;
        #endregion

        #region ctor
        public Console(InputButton bindConsoleButton, Margin consoleMargin, Material2DColored consoleBackground, Color consoleFontColor, int maxNumberOfLines)
        {
            // Set the Material2DColored
            this.consoleBackground = consoleBackground;
            // The console should be topmost
            this.consoleBackground.DrawLayer = Delta.ContentSystem.Rendering.RenderLayer.Front;

            // Set the margin and max. line count
            this.consoleMargin = consoleMargin;
            this.maxNumberOfLines = maxNumberOfLines;

            // Initialize the console dimension
            consoleDimension = new Rectangle();

            // Initialize empty history
            history = new List<string>();
            cmdHistory = new Stack<string>();

            // Initialize empty suggestions
            autoCompletionCache = new List<string>();

            // Initialize with empty string
            inputText = "";
            lastInputText = "";

            // Set the console font with the given color
            consoleFont = new Font(Font.Default, consoleFontColor, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Create a new instance of the CommandManager
            cmdManager = new CommandManager();

            // Register input commands
            registerInputCommands(bindConsoleButton);

            // Initialize calculation
            calculateConsoleDimension();
        }
        #endregion

        #region AddCmdToConsole
        public void AddCmdToConsole(MethodInfo method, object target)
        {
            cmdManager.AddCmd(method, target);
        }
        #endregion

        #region registerInputCommands
        private void registerInputCommands(InputButton bindConsoleButton)
        {
            // Show or hide the console
            Input.Commands["Console"]
                .AddTrigger(new CommandTrigger { Button = bindConsoleButton, State = InputState.Pressed })
                .Add(this, command =>
                {
                    // Invert value
                    showConsole = !showConsole;

                    // Workaround to delete all previous entered text
                    // http://forum.deltaengine.net/yaf_postsm2636_Input-Keyboard-HandleInput-alternative.aspx#post2636
                    var killText = "";
                    Input.Keyboard.HandleInput(ref killText);
                });

            // Evaluate and execute a entered command
            Input.Commands["ExecuteCmd"]
                .AddTrigger(new CommandTrigger { Button = InputButton.Enter, State = InputState.Pressed })
                .Add(this, command =>
                {
                    // Only execute when the console is visible
                    if (showConsole)
                    {
                        executeCmd();
                        cmdHistoryPosition = 0;
                    }
                });

            // Do autocompletion
            Input.Commands["AutoComplete"]
                .AddTrigger(new CommandTrigger { Button = InputButton.Tab, State = InputState.Pressed })
                .Add(this, command =>
                {
                    // Only execute when the console is visible and we have input to process
                    if (showConsole && !string.IsNullOrWhiteSpace(inputText))
                    {
                        // Try to autocomplete the input to a matching command
                        var text = cmdManager.AutoCompleteString(inputText);
                        inputText = text;
                        cmdHistoryPosition = 0;
                    }
                });

            // Step through history when pressing CursorUp or CursorDown
            Input.Commands["StepThroughHistory"]
                .AddTrigger(new CommandTrigger { Button = InputButton.CursorUp, State = InputState.Pressed })
                .AddTrigger(new CommandTrigger { Button = InputButton.CursorDown, State = InputState.Pressed })
                .Add(this, command =>
                {
                    if (showConsole)
                    {
                        if (command.Button == InputButton.CursorUp)
                        {
                            moveHistoryUp();
                        }
                        else
                        {
                            moveHistoryDown();
                        }
                    }
                });
        }
        #endregion

        #region moveHistoryUp
        private void moveHistoryUp()
        {
            // Check if the current index is not smaller then the number of items in our collection
            if (cmdHistoryPosition <= cmdHistory.Count() - 1)
            {
                if (cmdHistoryPosition == 0)
                {
                    // Take the first item and increment the counter
                    inputText = cmdHistory.FirstOrDefault();
                    cmdHistoryPosition++;
                }
                else
                {
                    // Increment the counter and take the current index
                    cmdHistoryPosition++;
                    inputText = cmdHistory.Skip(cmdHistoryPosition -1).Take(1).FirstOrDefault();
                }
            }
        }
        #endregion

        #region moveHistoryDown
        private void moveHistoryDown()
        {
            // Check if the current index is bigger than zero
            if (cmdHistoryPosition - 1 > 0)
            {
                // Decrement the counter and take the current index
                cmdHistoryPosition--;
                inputText = cmdHistory.Skip(cmdHistoryPosition -1).Take(1).FirstOrDefault();
            }
        }
        #endregion

        #region executeCmd
        private void executeCmd()
        {
            // Add to the history
            cmdHistory.Push(inputText);

            // We can't do anything with empty input
            if (string.IsNullOrWhiteSpace(inputText))
            {
                addHistory(">");
            }
            else
            {
                // Add entered command and result to the history
                addHistory("> " + inputText);
                addHistory(cmdManager.ExecuteCommand(inputText));
            }
            // Clear the input
            inputText = "";
        }
        #endregion

        #region addHistory
        private void addHistory(string text)
        {
            // Check for max. history count
            if (history.Count >= maxNumberOfLines)
            {
                // Remove oldest entry
                history.RemoveAt(0);
            }

            // Add text to the history
            history.Add(text);
        }
        #endregion

        #region Draw
        public void Draw()
        {
            //Check if the console should be rendered
            if (showConsole)
            {
                // Get keyboard input
                Input.Keyboard.HandleInput(ref _inputText);

                // Draw the console background and text
                calculateConsoleDimension();
                consoleBackground.Draw(consoleDimension);
                drawConsoleText();
                renderAutoCompletionList();
                drawAutoCompletion();
            }
        }
        #endregion

        #region drawConsoleText
        private void drawConsoleText()
        {
            // Iterate the history list
            for (int i = 0; i < history.Count; i++)
            {
                // Calculate the position to draw the text
                // For each item in it we incase the distance from top to the value of: fontHeight * itemPosition
                // Also add some space from the left and top with the value of fontHeight
                // With this, each item has is own column
                consoleFont.Draw(history[i], new Rectangle(consoleDimension.Left + fontHeight, consoleDimension.Top + (fontHeight * (i + 1)), consoleDimension.Width, fontHeight));
            }

            // Calculate the position of the current text to draw to enter like above
            // Draw the current entered text and add a underscore to his end
            consoleFont.Draw("> " + inputText + "_", new Rectangle(consoleDimension.Left + fontHeight, consoleDimension.Top + (fontHeight * (history.Count + 1)), consoleDimension.Width, fontHeight));
        }
        #endregion

        #region renderAutoCompletionList
        private void renderAutoCompletionList()
        {
            // Check if the input has been changed since last call
            if (inputText != lastInputText)
            {
                // Do we have any input?
                if (inputText.Length > 0)
                {
                    // Get all matching methods
                    autoCompletionCache = cmdManager.GetAutoCompletionList(inputText);
                }
                else
                {
                    // Nothing to do, just clear
                    autoCompletionCache.Clear();
                }

                // Sync input
                lastInputText = inputText;
            }
        }
        #endregion

        #region drawAutoCompletion
        private void drawAutoCompletion()
        {
            // Draw the autocompletion like drawConsoleText() does
            for (int i = 0; i < autoCompletionCache.Count; i++)
            {
                consoleFont.Draw(autoCompletionCache[i], new Rectangle(consoleDimension.Left + fontHeight, consoleDimension.Top + (fontHeight * (history.Count + i + 2)), consoleDimension.Width, fontHeight));
            }
        }
        #endregion

        #region drawConsole
        private void calculateConsoleDimension()
        {
            // Re-calculate the quadratic space of the consoleFont height
            fontHeight = ScreenSpace.ToQuadraticSpace(new Size(15f)).Height;

            // Render the console background with some distance to left, top, right and add extra height
            consoleDimension.Left = ScreenSpace.DrawArea.Left + consoleMargin.Left;
            consoleDimension.Top = ScreenSpace.DrawArea.Top + consoleMargin.Top;
            consoleDimension.Width = ScreenSpace.DrawArea.Right - consoleMargin.Right;
            // Calculate all the needed height together
            consoleDimension.Height = fontHeight * (history.Count + autoCompletionCache.Count + 3);
        }
        #endregion
    }
}