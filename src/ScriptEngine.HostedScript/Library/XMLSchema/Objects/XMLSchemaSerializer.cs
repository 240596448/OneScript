﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    internal class XMLSchemaSerializer
    {
        internal static IXSComponent CreateInstance(XmlSchemaObject xmlSchemaObject)
        {
            if (xmlSchemaObject is XmlSchemaExternal xmlExternal)
                return CreateIXSDirective(xmlExternal);

            else if (xmlSchemaObject is XmlSchemaAnnotated xmlAnnotated)
                return CreateIXSAnnotated(xmlAnnotated);

            else if (xmlSchemaObject is XmlSchemaAnnotation xmlAnnotation)
                return CreateXSAnnotation(xmlAnnotation);

            else if (xmlSchemaObject is XmlSchemaDocumentation xmlDocumentation)
                return new XSDocumentation(xmlDocumentation);

            else
                return null;
        }

        private static IXSDirective CreateIXSDirective(XmlSchemaExternal xmlSchemaExternal)
        {
            if (xmlSchemaExternal is XmlSchemaImport import)
                return new XSImport(import);

            else if (xmlSchemaExternal is XmlSchemaInclude include)
                return new XSInclude(include);

            else if (xmlSchemaExternal is XmlSchemaRedefine redefine)
                return new XSRedefine(redefine);

            throw RuntimeException.InvalidArgumentType();
        }

        internal static IXSType CreateIXSType(XmlSchemaType xmlType)
        {
            if (xmlType is XmlSchemaComplexType complexType)
                return new XSComplexTypeDefinition(complexType);

            else if (xmlType is XmlSchemaSimpleType simpleType)
                return new XSSimpleTypeDefinition(simpleType);

            throw RuntimeException.InvalidArgumentType();
        }

        private static IXSComponent CreateIXSAnnotated(XmlSchemaAnnotated xmlAnnotated)
        {
            if (xmlAnnotated is XmlSchemaType xmlType)
                return CreateIXSType(xmlType);

            else if (xmlAnnotated is XmlSchemaParticle xmlParticle)
            {
                if (string.IsNullOrEmpty(xmlParticle.MinOccursString) && string.IsNullOrEmpty(xmlParticle.MaxOccursString))
                    return CreateIXSFragment(xmlParticle);
                else
                    return new XSParticle(xmlParticle);
            }
            else if (xmlAnnotated is XmlSchemaFacet xmlFacet)
                return CreateIXSFacet(xmlFacet);

            else
                throw RuntimeException.InvalidArgumentType();
        }

        internal static IXSFragment CreateIXSFragment(XmlSchemaParticle xmlParticle)
        {
            if (xmlParticle is XmlSchemaElement element)
                return new XSElementDeclaration(element);

            else if (xmlParticle is XmlSchemaGroupBase groupBase)
                return new XSModelGroup(groupBase);

            throw RuntimeException.InvalidArgumentType();
        }

        internal static IXSFacet CreateIXSFacet(XmlSchemaFacet xmlFacet)
        {
            if (xmlFacet is XmlSchemaEnumerationFacet enumerationFacet)
                return new XSEnumerationFacet(enumerationFacet);

            throw RuntimeException.InvalidArgumentType();
        }

        internal static XSAnnotation CreateXSAnnotation(XmlSchemaAnnotation annotation)
            => new XSAnnotation(annotation);
    }
}
