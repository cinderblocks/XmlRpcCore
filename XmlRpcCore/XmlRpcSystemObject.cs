using System;
using System.Collections;
using System.Reflection;

namespace XmlRpcCore
{
    /// <summary> XML-RPC System object implementation of extended specifications.</summary>
    [XmlRpcExposed]
    public class XmlRpcSystemObject
    {
        private readonly XmlRpcServer _server;

        /// <summary>Constructor.</summary>
        /// <param name="server"><c>XmlRpcServer</c> server to be the system object for.</param>
        public XmlRpcSystemObject(XmlRpcServer server)
        {
            _server = server;
            server.Add("system", this);
            MethodHelp.Add(GetType().FullName + ".methodHelp", "Return a string description.");
        }

        /// <summary>Static <c>IDictionary</c> to hold mappings of method name to associated documentation String</summary>
        public static IDictionary MethodHelp { get; } = new Hashtable();

        /// <summary>Invoke a method on a given object.</summary>
        /// <remarks>
        ///     Using reflection, and respecting the <c>XmlRpcExposed</c> attribute,
        ///     invoke the <paramref>methodName</paramref> method on the <paramref>target</paramref>
        ///     instance with the <paramref>parameters</paramref> provided. All this packages other <c>Invoke</c> methods
        ///     end up calling this.
        /// </remarks>
        /// <returns><c>Object</c> the value the invoked method returns.</returns>
        /// <exception cref="XmlRpcException">
        ///     If method does not exist, is not exposed, parameters invalid, or invocation
        ///     results in an exception. Note, the <c>XmlRpcException.Code</c> will indicate cause.
        /// </exception>
        public static object Invoke(object target, string methodName, IList parameters)
        {
            if (target == null)
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_METHOD,
                    XmlRpcErrorCodes.SERVER_ERROR_METHOD_MSG + ": Invalid target object.");

            var type = target.GetType();
            var method = type.GetMethod(methodName);

            try
            {
                if (!XmlRpcExposedAttribute.ExposedMethod(target, methodName))
                    throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_METHOD,
                        XmlRpcErrorCodes.SERVER_ERROR_METHOD_MSG + ": Method " + methodName + " is not exposed.");
            }
            catch (MissingMethodException me)
            {
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_METHOD,
                    XmlRpcErrorCodes.SERVER_ERROR_METHOD_MSG + ": " + me.Message);
            }

            var args = new object[parameters.Count];

            var index = 0;
            foreach (var arg in parameters)
            {
                args[index] = arg;
                index++;
            }

            try
            {
                var retValue = method.Invoke(target, args);
                if (retValue == null)
                    throw new XmlRpcException(XmlRpcErrorCodes.APPLICATION_ERROR,
                        XmlRpcErrorCodes.APPLICATION_ERROR_MSG + ": Method returned NULL.");
                return retValue;
            }
            catch (XmlRpcException e)
            {
                throw e;
            }
            catch (ArgumentException ae)
            {
                Logger.WriteEntry(XmlRpcErrorCodes.SERVER_ERROR_PARAMS_MSG + ": " + ae.Message,
                    LogLevel.Information);
                var call = methodName + "( ";
                foreach (var o in args)
                {
                    call += o.GetType().Name;
                    call += " ";
                }
                call += ")";
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_PARAMS,
                    XmlRpcErrorCodes.SERVER_ERROR_PARAMS_MSG + ": Arguement type mismatch invoking " + call);
            }
            catch (TargetParameterCountException tpce)
            {
                Logger.WriteEntry(XmlRpcErrorCodes.SERVER_ERROR_PARAMS_MSG + ": " + tpce.Message,
                    LogLevel.Information);
                throw new XmlRpcException(XmlRpcErrorCodes.SERVER_ERROR_PARAMS,
                    XmlRpcErrorCodes.SERVER_ERROR_PARAMS_MSG + ": Arguement count mismatch invoking " + methodName);
            }
            catch (TargetInvocationException tie)
            {
                throw new XmlRpcException(XmlRpcErrorCodes.APPLICATION_ERROR,
                    XmlRpcErrorCodes.APPLICATION_ERROR_MSG + " Invoked method " + methodName + ": " + tie.Message);
            }
        }

        /// <summary>List methods available on all handlers of this server.</summary>
        /// <returns><c>IList</c> An array of <c>Strings</c>, each <c>String</c> will have form "object.method".</returns>
        [XmlRpcExposed]
        public IList listMethods()
        {
            IList methods = new ArrayList();
            bool considerExposure;

            foreach (DictionaryEntry handlerEntry in _server)
            {
                considerExposure = XmlRpcExposedAttribute.IsExposed(handlerEntry.Value.GetType());

                foreach (var mi in handlerEntry.Value.GetType().GetMembers())
                {
                    if (mi.MemberType != MemberTypes.Method)
                        continue;

                    if (!((MethodInfo) mi).IsPublic)
                        continue;

                    if (considerExposure && !XmlRpcExposedAttribute.IsExposed(mi))
                        continue;

                    methods.Add(handlerEntry.Key + "." + mi.Name);
                }
            }

            return methods;
        }

        /// <summary>Given a method name return the possible signatures for it.</summary>
        /// <param name="name"><c>String</c> The object.method name to look up.</param>
        /// <returns><c>IList</c> Of arrays of signatures.</returns>
        [XmlRpcExposed]
        public IList methodSignature(string name)
        {
            IList signatures = new ArrayList();
            var index = name.IndexOf('.');

            if (index < 0)
                return signatures;

            var oName = name.Substring(0, index);
            var obj = _server[oName];

            if (obj == null)
                return signatures;

            var mi = obj.GetType().GetMember(name.Substring(index + 1));

            if (mi.Length != 1) // for now we want a single signature
                return signatures;

            MethodInfo method;

            try
            {
                method = (MethodInfo) mi[0];
            }
            catch (Exception e)
            {
                Logger.WriteEntry("Attempted methodSignature call on " + mi[0] + " caused: " + e,
                    LogLevel.Information);
                return signatures;
            }

            if (!method.IsPublic)
                return signatures;

            IList signature = new ArrayList();
            signature.Add(method.ReturnType.Name);

            foreach (var param in method.GetParameters())
                signature.Add(param.ParameterType.Name);


            signatures.Add(signature);

            return signatures;
        }

        /// <summary>Help for given method signature. Not implemented yet.</summary>
        /// <param name="name"><c>String</c> The object.method name to look up.</param>
        /// <returns><c>String</c> help text. Rich HTML text.</returns>
        [XmlRpcExposed]
        public string methodHelp(string name)
        {
            string help = null;

            try
            {
                help = (string) MethodHelp[_server.MethodName(name)];
            }
            catch (XmlRpcException e)
            {
                throw e;
            }
            catch (Exception)
            {
                /* ignored */
            }

            if (help == null)
                help = "No help available for: " + name;

            return help;
        }

        /// <summary>Boxcarring support method.</summary>
        /// <param name="calls"><c>IList</c> of calls</param>
        /// <returns><c>ArrayList</c> of results/faults.</returns>
        [XmlRpcExposed]
        public IList multiCall(IList calls)
        {
            IList responses = new ArrayList();
            var fault = new XmlRpcResponse();

            foreach (IDictionary call in calls)
                try
                {
                    var req = new XmlRpcRequest((string) call[XmlRpcXmlTokens.METHOD_NAME],
                        (ArrayList) call[XmlRpcXmlTokens.PARAMS]);
                    var results = _server.Invoke(req);
                    IList response = new ArrayList();
                    response.Add(results);
                    responses.Add(response);
                }
                catch (XmlRpcException e)
                {
                    fault.SetFault(e.FaultCode, e.FaultString);
                    responses.Add(fault.Value);
                }
                catch (Exception e2)
                {
                    fault.SetFault(XmlRpcErrorCodes.APPLICATION_ERROR,
                        XmlRpcErrorCodes.APPLICATION_ERROR_MSG + ": " + e2.Message);
                    responses.Add(fault.Value);
                }

            return responses;
        }
    }
}