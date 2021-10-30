using System;
using System.IO;
using System.Web.Mvc;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AECMIS.MVC.Binders
{
    public class FromJsonAttribute : CustomModelBinderAttribute
    {
        public override IModelBinder GetBinder()
        {
            return new JsonModelBinder();
        }

        private class JsonModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                var stringified = controllerContext.HttpContext.Request[bindingContext.ModelName];
                
                if (string.IsNullOrEmpty(stringified))
                    return null;
                return JsonConvert.DeserializeObject(stringified, bindingContext.ModelType,
                    new ContactConverter(),new AddressConverter(), new ImageConverter());
            }
        }
    }

    public class ContactConverter : CustomCreationConverter<IContact>
    {
        public override IContact Create(Type objectType)
        {
            return new Contact();
        }
    }

    public class AddressConverter : CustomCreationConverter<IAddress>
    {
        public override IAddress Create(Type objectType)
        {
            return new Address();
        }

        
    }

    public class ImageConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            // var m = new MemoryStream(Convert.FromBase64String((string) reader.Value));
            return reader == null || reader.Value == null ? null : Convert.FromBase64String((string) reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            byte[] bmp = (byte[]) value;
            MemoryStream m = new MemoryStream();
            //bmp.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);

            writer.WriteValue(Convert.ToBase64String(m.ToArray()));
        }
    }

    public class NHibernateContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(NHibernate.Proxy.INHibernateProxy).IsAssignableFrom(objectType))
                return base.CreateContract(objectType.BaseType);
            else
                return base.CreateContract(objectType);
        }
    }

    public static class JsonConverter
    {
        public static string SerializeObject(this object objectToSerialize)
        {
            var serializer = new JsonSerializer
                                 {
                                     ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                     ContractResolver = new NHibernateContractResolver()
                                 };
            var stringWriter = new StringWriter();
            JsonWriter jsonWriter = new JsonTextWriter(stringWriter);
            serializer.Serialize(jsonWriter, objectToSerialize);
            return stringWriter.ToString();
        }
    }

}