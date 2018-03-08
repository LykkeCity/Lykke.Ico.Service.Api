using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lykke.Service.IcoApi.Core.Emails
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EmailDataAttribute : Attribute
    {
        private static Dictionary<string, object> _dataModels;

        public static Dictionary<string, object> DataModels
        {
            get
            {
                if (_dataModels == null)
                {
                    _dataModels = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic)
                        .SelectMany(a => a.GetExportedTypes())
                        .Where(t => 
                            t.IsClass && 
                            !t.IsAbstract && 
                            t.GetConstructor(Type.EmptyTypes) != null && 
                            t.CustomAttributes.Any(c => c.AttributeType == typeof(EmailDataAttribute)))
                        .ToDictionary(
                            t => t.GetCustomAttribute<EmailDataAttribute>().TemplateId,
                            t => Activator.CreateInstance(t)
                        );
                }

                return _dataModels;
            }
        }

        public EmailDataAttribute(string templateId) => TemplateId = templateId;

        public string TemplateId { get; }

        public static object FindDataModel(string templateId)
        {
            throw new NotImplementedException();
        }
    }
}
