using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Delta.Console
{
    /// <summary>
    /// Evaluate command strings and executes delegates
    /// </summary>
    class CommandManager
    {
        #region Private
        /// <summary>
        /// Save all available commands used by the console
        /// </summary>
        private List<Delegate> delegateList;
        #endregion

        #region ctor
        public CommandManager()
        {
            // Initialize  
            delegateList = new List<Delegate>();
        }
        #endregion

        #region ExecuteCommand
        public string ExecuteCommand(string command)
        {
            // Command name and parameters are seperated by whitespaces
            var splittedCmd = command.Split(' ');

            // Try to get the first delegate that has the name of the command (ignore upper/lower case)
            var method = delegateList.Select(x => x)
                .Where(x => x.Method.GetCustomAttributes(typeof(ConsoleCommandAttribute), true)
                    .Cast<ConsoleCommandAttribute>()
                    .FirstOrDefault().Name.Equals(splittedCmd[0], StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

            // Was the command found?
            if (method != null)
            {
                // Get parameters needed for this method
                var parameter = method.Method.GetParameters();

                // Do a simple check if the number of parameters matchs
                if (parameter.Length == splittedCmd.Length - 1)
                {
                    // Check for parameterless command
                    if (parameter.Length == 0)
                    {
                        // Just invoke the delegate without any parameters
                        return invokeDelegate(method, null);
                    }
                    else
                    {
                        // List to save parameters to call the method
                        var paramObjLst = new List<object>();

                        // Iterate all parameters
                        for (int i = 0; i < parameter.Length; i++)
                        {
                            try
                            {
                                // Try to convert the input string to the target type.
                                // Because the first item in splittedCmd is the command name, we have to increment the position +1
                                paramObjLst.Add(Convert.ChangeType(splittedCmd[i + 1], parameter[i].ParameterType));
                            }
                            catch (Exception ex)
                            {
                                // Error converting types (parameter format not matching?)
                                return string.Format("=> Error: Can't process parameter no. {0}: {1}", i + 1, ex.Message);
                            }
                        }
                        // Invoke the delegate with our parameters
                        return invokeDelegate(method, paramObjLst.ToArray());
                    }
                }
                else
                {
                    return string.Format("=> Error: The command has {0} parameters, but you entered {1}", parameter.Length, splittedCmd.Length - 1);
                }
            }
            else
            {
                return string.Format("=> Error: Unknow console command \"{0}\"! (use \"list\" command to view available commands)", splittedCmd[0]);
            }
        }
        #endregion

        #region invokeDelegate
        private string invokeDelegate(Delegate method, params object[] args)
        {
            try
            {
                // Try to invoke the delegate
                var result = Convert.ToString(method.DynamicInvoke(args));

                // Check for empty result (e.g. the method return void)?
                if (string.IsNullOrWhiteSpace(result))
                {
                    return "=> Command executed";
                }
                else
                {
                    return string.Format("=> Result: {0}", result);
                }
            }
            catch (Exception ex)
            {
                // Error while invoking, print exception
                return string.Format("=> Error: Exception while invoking the command: {0}", ex.Message);
            }
        }
        #endregion

        #region GetAutoCompletionList
        public List<string> GetAutoCompletionList(string input)
        {
            //Find all methods that start with the given input (ignore upper/lower case)
            var cmdList = delegateList.Select(x => x.Method)
                .Where(x => x.GetCustomAttributes(typeof(ConsoleCommandAttribute), true)
                    .Cast<ConsoleCommandAttribute>()
                    .FirstOrDefault().Name.StartsWith(input, true, null)).ToList();

            // Initlize a new list
            var autoCompletionList = new List<string>();

            foreach (var item in cmdList)
            {
                // Get the name of the method
                var name = (item.GetCustomAttributes(typeof(ConsoleCommandAttribute), true).FirstOrDefault() as ConsoleCommandAttribute).Name;
                // Get all parameters for this method
                var parameter = item.GetParameters().Select(x => x.ParameterType.Name);

                // Do we have parameter for this method?
                if (parameter.Count() > 0)
                {
                    // Concat the method name and all parameters together
                    autoCompletionList.Add(name + " " + parameter.Aggregate((x, y) => x + " " + y));
                }
                else
                {
                    // The method doesen't have any parameter, only add the name
                    autoCompletionList.Add(name);
                }
            }

            // Sort alphabetically
            return autoCompletionList.OrderBy(x => x).ToList();
        }
        #endregion

        #region AutoComplete
        public string AutoComplete(string input)
        {
            //Find all methods that start with the given input (ignore upper/lower case)
            var cmdList = delegateList.Select(x => x.Method)
                .Where(x => x.GetCustomAttributes(typeof(ConsoleCommandAttribute), true)
                    .Cast<ConsoleCommandAttribute>()
                    .FirstOrDefault().Name.StartsWith(input, true, null)).ToList();

            var nearestCmd = cmdList.Select(x => (x.GetCustomAttributes(typeof(ConsoleCommandAttribute), true).FirstOrDefault() as ConsoleCommandAttribute).Name).OrderBy(x => x);

            string returnCmd = "";



            return returnCmd;
        }
        #endregion

        #region createDelegate
        private static Delegate createDelegate(MethodInfo method, object target)
        {
            // Builds a Delegate from a MethodInfo.
            return Delegate.CreateDelegate
            (
                Expression.GetDelegateType
                (
                    method.GetParameters()
                        .Select(p => p.ParameterType)
                        .Concat(new Type[] { method.ReturnType })
                        .ToArray()
                ), target, method
            );
        }
        #endregion

        #region AddCmdToConsole
        public void AddCmd(MethodInfo method, object target)
        {
            // NULL check
            if (method == null)
            {
                throw new NullReferenceException("MethodInfo can't be NULL!");
            }

            // Check if the ConsoleCommandAttribute is attached to the method
            if (method.GetCustomAttributes(typeof(ConsoleCommandAttribute), true).Length == 0)
            {
                throw new ArgumentException("ConsoleCommandAttribute is missing, please add it to your method!");
            }

            // Create a deleggate from our MethodInfo and add it to the delegateList
            delegateList.Add(createDelegate(method, target));
        }
        #endregion
    }
}