﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Zen.Web.App.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Zen.Web.App.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*
        ///*
        ///*  Push Notifications codelab
        ///*  Copyright 2015 Google Inc. All rights reserved.
        ///*
        ///*  Licensed under the Apache License, Version 2.0 (the &quot;License&quot;);
        ///*  you may not use this file except in compliance with the License.
        ///*  You may obtain a copy of the License at
        ///*
        ///*      https://www.apache.org/licenses/LICENSE-2.0
        ///*
        ///*  Unless required by applicable law or agreed to in writing, software
        ///*  distributed under the License is distributed on an &quot;AS IS&quot; BASIS,
        ///*  WITHOUT WARRANTIES OR CONDITIONS OF [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string pushServiceWorker {
            get {
                return ResourceManager.GetString("pushServiceWorker", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var loc = window.location.href + &quot;&quot;;
        ///
        ///if (loc.indexOf(&quot;http://&quot;) === 0) {
        ///    window.location.href = loc.replace(&quot;http://&quot;, &quot;https://&quot;);
        ///} else {
        ///
        ///    function injectJavascript(res) {
        ///        document.write(&apos;\x3Cscript src=&quot;&apos; + applyQuery(res, globalAppSettings.vTag) + &apos;&quot;&gt;\x3C/script&gt;&apos;);
        ///    };
        ///
        ///    function injectPreconnect(res) {
        ///        document.write(&apos;\x3Clink rel=&quot;preconnect&quot; href=&quot;&apos; + applyQuery(res, globalAppSettings.vTag) + &apos;&quot; /&gt;&apos;);
        ///    };
        ///
        ///    function injectCss(res) {
        ///        docume [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string template {
            get {
                return ResourceManager.GetString("template", resourceCulture);
            }
        }
    }
}
