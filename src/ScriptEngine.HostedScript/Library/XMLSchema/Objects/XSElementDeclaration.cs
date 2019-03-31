﻿using System;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.Xml;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [ContextClass("ОбъявлениеЭлементаXS", "XSElementDeclaration")]
    public class XSElementDeclaration : AutoContext<XSElementDeclaration>, IXSFragment, IXSNamedComponent
    {
        private readonly XmlSchemaElement _element;
        private XSAnnotation _annotation;
        private XMLExpandedName _typeName;
        private XMLExpandedName _refName;
        private XSConstraint _constraint;
        private IXSType _schemaType;
        private IValue _value;

        private XSElementDeclaration() => _element = new XmlSchemaElement();

        #region OneScript

        #region Properties

        [ContextProperty("Аннотация", "Annotation")]
        public XSAnnotation Annotation
        {
            get => _annotation;
            set
            {
                _annotation = value;
                _element.Annotation = value.InternalObject;
            }
        }

        [ContextProperty("Компоненты", "Components")]
        public XSComponentFixedList Components { get; }

        [ContextProperty("Контейнер", "Container")]
        public IXSComponent Container { get; private set; }

        [ContextProperty("КорневойКонтейнер", "RootContainer")]
        public IXSComponent RootContainer { get; private set; }

        [ContextProperty("Схема", "Schema")]
        public XMLSchema Schema => RootContainer.Schema;

        [ContextProperty("ТипКомпоненты", "ComponentType")]
        public XSComponentType ComponentType => XSComponentType.ElementDeclaration;

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string URIПространстваИмен => _element.SourceUri;

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get => _element.Name;
            set => _element.Name = value;
        }

        [ContextProperty("АнонимноеОпределениеТипа", "AnonymousTypeDefinition")]
        public IXSType AnonymousTypeDefinition
        {
            get => _schemaType;
            set
            {
                _schemaType = value;
                _element.SchemaType = _schemaType.SchemaObject as XmlSchemaType;
            }
        }

        [ContextProperty("Значение", "Value")]
        public IValue Value
        {
            get => _value;
            set
            {
                _value = value;
                if (_constraint == XSConstraint.Fixed)
                    _element.FixedValue = XMLSchema.XMLStringIValue(_value);
                else
                    _element.DefaultValue = XMLSchema.XMLStringIValue(_value);
            }
        }

        [ContextProperty("ИмяТипа", "TypeName")]
        public XMLExpandedName TypeName
        {
            get => _typeName;
            set
            {
               _typeName = value;
                _element.SchemaTypeName = _typeName.NativeValue;
            }
        }

        [ContextProperty("ЛексическоеЗначение", "LexicalValue")]
        public string LexicalValue
        {
            get => _constraint == XSConstraint.Fixed ? _element.FixedValue : _element.DefaultValue;
            set
            {
                if (_constraint == XSConstraint.Fixed)
                    _element.FixedValue = value;
                else
                    _element.DefaultValue = value;
            }
        }

        [ContextProperty("ОбластьВидимости", "Scope")]
        public XSElementDeclaration Scope { get; }

        [ContextProperty("Ограничение", "Constraint")]
        public XSConstraint Constraint
        {
            get => _constraint;
            set
            {
                _constraint = value;
                if (_constraint == XSConstraint.Default)
                    _element.FixedValue = null;

                else if (_constraint == XSConstraint.Fixed)
                    _element.DefaultValue = null;

                else
                {
                    _element.FixedValue = null;
                    _element.DefaultValue = null;
                }
            }
        }

        [ContextProperty("Ссылка", "Reference")]
        public XMLExpandedName Reference
        {
            get => _refName;
            set
            {
                _refName = value;
                _element.RefName = _refName.NativeValue;
            }
        }

        [ContextProperty("Форма", "Form")]
        public XSForm Form
        {
            get => XSForm.FromNativeValue(_element.Form);
            set => _element.Form = XSForm.ToNativeValue(value);
        }

        //ЭтоГлобальноеОбъявление(IsGlobal)
        //ЭтоСсылка(IsReference)

        [ContextProperty("Абстрактный", "Abstract")]
        public bool Abstract
        {
            get => _element.IsAbstract;
            set => _element.IsAbstract = value;
        }

        //Блокировка(Block)

        [ContextProperty("ВозможноПустой", "Nillable")]
        public bool Nillable
        {
            get => _element.IsNillable;
            set => _element.IsNillable = value;
        }

        //Завершенность(Final)
        //ИсключенияГруппПодстановки(SubstitutionGroupExclusions)
        //НедопустимыеПодстановки(DisallowedSubstitutions)
        //ОграниченияИдентичности(IdentityConstraints)

        [ContextProperty("ПрисоединениеКГруппеПодстановки", "SubstitutionGroupAffiliation")]
        public XMLExpandedName SubstitutionGroupAffiliation
        {
            get => _typeName;
            set
            {
                _typeName = value;
                _element.SubstitutionGroup = _typeName.NativeValue;
            }
        }

        #endregion

        #region Methods

        [ContextMethod("КлонироватьКомпоненту", "CloneComponent")]
        public IXSComponent CloneComponent(bool recursive = true) => throw new NotImplementedException();

        [ContextMethod("ОбновитьЭлементDOM", "UpdateDOMElement")]
        public void UpdateDOMElement() => throw new NotImplementedException();

        [ContextMethod("Содержит", "Contains")]
        public bool Contains(IXSComponent component) => throw new NotImplementedException();

        //ОпределениеТипа(TypeDefinition)
        //РазрешитьСсылку(ResolveReference)
        //ЭтоОбъявлениеЗациклено(IsCircular)

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XSElementDeclaration Constructor() => new XSElementDeclaration();

        #endregion

        #endregion

        #region IXSComponent

        XmlSchemaObject IXSComponent.SchemaObject => _element;

        void IXSComponent.BindToContainer(IXSComponent rootContainer, IXSComponent container)
        {
            RootContainer = rootContainer;
            Container = container;
        }

        #endregion
    }
}
