using System.Web.Mvc;
using System.Xml.Serialization;

namespace Disco.Web.Extensions
{
    /// <summary>
    /// Action result that serializes the specified object into XML and outputs it to the response stream.
    /// <example>
    /// <![CDATA[
    /// public XmlResult AsXml() {
    ///		List<Person> people = _peopleService.GetPeople();
    ///		return new XmlResult(people);
    /// }
    /// ]]>
    /// </example>
    /// </summary>
    public class XmlResult : ActionResult
    {
        private object _objectToSerialize;
        private XmlAttributeOverrides _xmlAttribueOverrides;

        /// <summary>
        /// Creates a new instance of the XmlResult class.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize to XML.</param>
        public XmlResult(object objectToSerialize)
        {
            _objectToSerialize = objectToSerialize;
        }

        /// <summary>
        /// Creates a new instance of the XMLResult class.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize to XML.</param>
        /// <param name="xmlAttributeOverrides"></param>
        public XmlResult(object objectToSerialize, XmlAttributeOverrides xmlAttributeOverrides)
        {
            _objectToSerialize = objectToSerialize;
            _xmlAttribueOverrides = xmlAttributeOverrides;
        }

        /// <summary>
        /// The object to be serialized to XML.
        /// </summary>
        public object ObjectToSerialize
        {
            get { return _objectToSerialize; }
        }

        /// <summary>
        /// Serialises the object that was passed into the constructor to XML and writes the corresponding XML to the result stream.
        /// </summary>
        /// <param name="context">The controller context for the current request.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (_objectToSerialize != null)
            {
                var xs = (_xmlAttribueOverrides == null) ?
                    new XmlSerializer(_objectToSerialize.GetType()) :
                    new XmlSerializer(_objectToSerialize.GetType(), _xmlAttribueOverrides);
                context.HttpContext.Response.ContentType = "text/xml";
                xs.Serialize(context.HttpContext.Response.Output, _objectToSerialize);
            }
        }

    }
}