/*!
    Sonoma.js Core
    Version: 3.0.0-DEV
    Date: 2017-08-07
*/
; (function () {

(function(t,e){"object"==typeof exports&&"undefined"!=typeof module?e(exports):"function"==typeof define&&define.amd?define(["exports"],e):e(t.RSVP=t.RSVP||{})})(this,function(t){"use strict";function e(t,e){for(var r=0,n=t.length;r<n;r++)if(t[r]===e)return r;return-1}function r(t){var e=t._promiseCallbacks;e||(e=t._promiseCallbacks={});return e}function n(t,e){if(2!==arguments.length)return jt[t];jt[t]=e}function o(t){var e=typeof t;return null!==t&&("object"===e||"function"===e)}function i(t){return"function"==typeof t}function u(t){return null!==t&&"object"==typeof t}function a(t){return null!==t&&"object"==typeof t}function s(){setTimeout(function(){for(var t=0;t<St.length;t++){var e=St[t],r=e.payload;r.guid=r.key+r.id;r.childGuid=r.key+r.childId;r.error&&(r.stack=r.error.stack);jt.trigger(e.name,e.payload)}St.length=0},50)}function c(t,e,r){1===St.push({name:t,payload:{key:e._guidKey,id:e._id,eventName:t,detail:e._result,childId:r&&r._id,label:e._label,timeStamp:Ot(),error:jt["instrument-with-stack"]?new Error(e._label):null}})&&s()}function f(t,e){var r=this;if(t&&"object"==typeof t&&t.constructor===r)return t;var n=new r(h,e);m(n,t);return n}function l(){return new TypeError("A promises callback cannot return that same promise.")}function h(){}function p(t){try{return t.then}catch(e){kt.error=e;return kt}}function y(t,e,r,n){try{t.call(e,r,n)}catch(o){return o}}function v(t,e,r){jt.async(function(t){var n=!1,o=y(r,e,function(r){if(!n){n=!0;e!==r?m(t,r,void 0):b(t,r)}},function(e){if(!n){n=!0;g(t,e)}},"Settle: "+(t._label||" unknown promise"));if(!n&&o){n=!0;g(t,o)}},t)}function d(t,e){if(e._state===Pt)b(t,e._result);else if(e._state===Rt){e._onError=null;g(t,e._result)}else j(e,void 0,function(r){e!==r?m(t,r,void 0):b(t,r)},function(e){return g(t,e)})}function _(t,e,r){var n=e.constructor===t.constructor&&r===P&&t.constructor.resolve===f;if(n)d(t,e);else if(r===kt){g(t,kt.error);kt.error=null}else i(r)?v(t,e,r):b(t,e)}function m(t,e){t===e?b(t,e):o(e)?_(t,e,p(e)):b(t,e)}function w(t){t._onError&&t._onError(t._result);E(t)}function b(t,e){if(t._state===At){t._result=e;t._state=Pt;0===t._subscribers.length?jt.instrument&&c("fulfilled",t):jt.async(E,t)}}function g(t,e){if(t._state===At){t._state=Rt;t._result=e;jt.async(w,t)}}function j(t,e,r,n){var o=t._subscribers,i=o.length;t._onError=null;o[i]=e;o[i+Pt]=r;o[i+Rt]=n;0===i&&t._state&&jt.async(E,t)}function E(t){var e=t._subscribers,r=t._state;jt.instrument&&c(r===Pt?"fulfilled":"rejected",t);if(0!==e.length){for(var n=void 0,o=void 0,i=t._result,u=0;u<e.length;u+=3){n=e[u];o=e[u+r];n?S(r,n,o,i):o(i)}t._subscribers.length=0}}function T(){this.error=null}function O(t,e){try{return t(e)}catch(r){xt.error=r;return xt}}function S(t,e,r,n){var o=i(r),u=void 0,a=void 0;if(o){u=O(r,n);if(u===xt){a=u.error;u.error=null}else if(u===e){g(e,l());return}}else u=n;e._state!==At||(o&&void 0===a?m(e,u):void 0!==a?g(e,a):t===Pt?b(e,u):t===Rt&&g(e,u))}function A(t,e){var r=!1;try{e(function(e){if(!r){r=!0;m(t,e)}},function(e){if(!r){r=!0;g(t,e)}})}catch(n){g(t,n)}}function P(t,e,r){var n=this,o=n._state;if(o===Pt&&!t||o===Rt&&!e){jt.instrument&&c("chained",n,n);return n}n._onError=null;var i=new n.constructor(h,r),u=n._result;jt.instrument&&c("chained",n,i);if(o===At)j(n,i,t,e);else{var a=o===Pt?t:e;jt.async(function(){return S(o,i,a,u)})}return i}function R(t,e,r){return t===Pt?{state:"fulfilled",value:r}:{state:"rejected",reason:r}}function k(t,e){return Tt(t)?new Mt(this,t,(!0),e).promise:this.reject(new TypeError("Promise.all must be called with an array"),e)}function x(t,e){var r=this,n=new r(h,e);if(!Tt(t)){g(n,new TypeError("Promise.race must be called with an array"));return n}for(var o=0;n._state===At&&o<t.length;o++)j(r.resolve(t[o]),void 0,function(t){return m(n,t)},function(t){return g(n,t)});return n}function M(t,e){var r=this,n=new r(h,e);g(n,t);return n}function C(){throw new TypeError("You must pass a resolver function as the first argument to the promise constructor")}function I(){throw new TypeError("Failed to construct 'Promise': Please use the 'new' operator, this object constructor cannot be called as a function.")}function N(){this.value=void 0}function V(t){try{return t.then}catch(e){Vt.value=e;return Vt}}function D(t,e,r){try{t.apply(e,r)}catch(n){Vt.value=n;return Vt}}function K(t,e){for(var r={},n=t.length,o=new Array(n),i=0;i<n;i++)o[i]=t[i];for(var u=0;u<e.length;u++){var a=e[u];r[a]=o[u+1]}return r}function U(t){for(var e=t.length,r=new Array(e-1),n=1;n<e;n++)r[n-1]=t[n];return r}function q(t,e){return{then:function(r,n){return t.call(e,r,n)}}}function F(t,e){var r=function(){for(var r=this,n=arguments.length,o=new Array(n+1),i=!1,u=0;u<n;++u){var a=arguments[u];if(!i){i=W(a);if(i===Dt){var s=new Nt(h);g(s,Dt.value);return s}i&&i!==!0&&(a=q(i,a))}o[u]=a}var c=new Nt(h);o[n]=function(t,r){t?g(c,t):void 0===e?m(c,r):e===!0?m(c,U(arguments)):Tt(e)?m(c,K(arguments,e)):m(c,r)};return i?L(c,o,t,r):G(c,o,t,r)};r.__proto__=t;return r}function G(t,e,r,n){var o=D(r,n,e);o===Vt&&g(t,o.value);return t}function L(t,e,r,n){return Nt.all(e).then(function(e){var o=D(r,n,e);o===Vt&&g(t,o.value);return t})}function W(t){return!(!t||"object"!=typeof t)&&(t.constructor===Nt||V(t))}function Y(t,e){return Nt.all(t,e)}function $(t,e){if(!t)throw new ReferenceError("this hasn't been initialised - super() hasn't been called");return!e||"object"!=typeof e&&"function"!=typeof e?t:e}function z(t,e){if("function"!=typeof e&&null!==e)throw new TypeError("Super expression must either be null or a function, not "+typeof e);t.prototype=Object.create(e&&e.prototype,{constructor:{value:t,enumerable:!1,writable:!0,configurable:!0}});e&&(Object.setPrototypeOf?Object.setPrototypeOf(t,e):t.__proto__=e)}function B(t,e){return Tt(t)?new Kt(Nt,t,e).promise:Nt.reject(new TypeError("Promise.allSettled must be called with an array"),e)}function H(t,e){return Nt.race(t,e)}function J(t,e){if(!t)throw new ReferenceError("this hasn't been initialised - super() hasn't been called");return!e||"object"!=typeof e&&"function"!=typeof e?t:e}function Q(t,e){if("function"!=typeof e&&null!==e)throw new TypeError("Super expression must either be null or a function, not "+typeof e);t.prototype=Object.create(e&&e.prototype,{constructor:{value:t,enumerable:!1,writable:!0,configurable:!0}});e&&(Object.setPrototypeOf?Object.setPrototypeOf(t,e):t.__proto__=e)}function X(t,e){return u(t)?new qt(Nt,t,e).promise:Nt.reject(new TypeError("Promise.hash must be called with an object"),e)}function Z(t,e){if(!t)throw new ReferenceError("this hasn't been initialised - super() hasn't been called");return!e||"object"!=typeof e&&"function"!=typeof e?t:e}function tt(t,e){if("function"!=typeof e&&null!==e)throw new TypeError("Super expression must either be null or a function, not "+typeof e);t.prototype=Object.create(e&&e.prototype,{constructor:{value:t,enumerable:!1,writable:!0,configurable:!0}});e&&(Object.setPrototypeOf?Object.setPrototypeOf(t,e):t.__proto__=e)}function et(t,e){return u(t)?new Ft(Nt,t,(!1),e).promise:Nt.reject(new TypeError("RSVP.hashSettled must be called with an object"),e)}function rt(t){setTimeout(function(){throw t});throw t}function nt(t){var e={resolve:void 0,reject:void 0};e.promise=new Nt(function(t,r){e.resolve=t;e.reject=r},t);return e}function ot(t,e,r){return Tt(t)?i(e)?Nt.all(t,r).then(function(t){for(var n=t.length,o=new Array(n),i=0;i<n;i++)o[i]=e(t[i]);return Nt.all(o,r)}):Nt.reject(new TypeError("RSVP.map expects a function as a second argument"),r):Nt.reject(new TypeError("RSVP.map must be called with an array"),r)}function it(t,e){return Nt.resolve(t,e)}function ut(t,e){return Nt.reject(t,e)}function at(t,e){return Nt.all(t,e)}function st(t,e){return Nt.resolve(t,e).then(function(t){return at(t,e)})}function ct(t,e,r){if(!(Tt(t)||u(t)&&void 0!==t.then))return Nt.reject(new TypeError("RSVP.filter must be called with an array or promise"),r);if(!i(e))return Nt.reject(new TypeError("RSVP.filter expects function as a second argument"),r);var n=Tt(t)?at(t,r):st(t,r);return n.then(function(t){for(var n=t.length,o=new Array(n),i=0;i<n;i++)o[i]=e(t[i]);return at(o,r).then(function(e){for(var r=new Array(n),o=0,i=0;i<n;i++)if(e[i]){r[o]=t[i];o++}r.length=o;return r})})}function ft(t,e){Ht[Gt]=t;Ht[Gt+1]=e;Gt+=2;2===Gt&&Jt()}function lt(){var t=process.nextTick,e=process.versions.node.match(/^(?:(\d+)\.)?(?:(\d+)\.)?(\*|\d+)$/);Array.isArray(e)&&"0"===e[1]&&"10"===e[2]&&(t=setImmediate);return function(){return t(dt)}}function ht(){return"undefined"!=typeof Lt?function(){Lt(dt)}:vt()}function pt(){var t=0,e=new $t(dt),r=document.createTextNode("");e.observe(r,{characterData:!0});return function(){return r.data=t=++t%2}}function yt(){var t=new MessageChannel;t.port1.onmessage=dt;return function(){return t.port2.postMessage(0)}}function vt(){return function(){return setTimeout(dt,1)}}function dt(){for(var t=0;t<Gt;t+=2){var e=Ht[t],r=Ht[t+1];e(r);Ht[t]=void 0;Ht[t+1]=void 0}Gt=0}function _t(){try{var t=require,e=t("vertx");Lt=e.runOnLoop||e.runOnContext;return ht()}catch(r){return vt()}}function mt(t,e,r){e in t?Object.defineProperty(t,e,{value:r,enumerable:!0,configurable:!0,writable:!0}):t[e]=r;return t}function wt(){jt.on.apply(jt,arguments)}function bt(){jt.off.apply(jt,arguments)}var gt={mixin:function(t){t.on=this.on;t.off=this.off;t.trigger=this.trigger;t._promiseCallbacks=void 0;return t},on:function(t,n){if("function"!=typeof n)throw new TypeError("Callback must be a function");var o=r(this),i=void 0;i=o[t];i||(i=o[t]=[]);e(i,n)===-1&&i.push(n)},off:function(t,n){var o=r(this),i=void 0,u=void 0;if(n){i=o[t];u=e(i,n);u!==-1&&i.splice(u,1)}else o[t]=[]},trigger:function(t,e,n){var o=r(this),i=void 0,u=void 0;if(i=o[t])for(var a=0;a<i.length;a++){u=i[a];u(e,n)}}},jt={instrument:!1};gt.mixin(jt);var Et=void 0;Et=Array.isArray?Array.isArray:function(t){return"[object Array]"===Object.prototype.toString.call(t)};var Tt=Et,Ot=Date.now||function(){return(new Date).getTime()},St=[],At=void 0,Pt=1,Rt=2,kt=new T,xt=new T,Mt=function(){function t(t,e,r,n){this._instanceConstructor=t;this.promise=new t(h,n);this._abortOnReject=r;this._init.apply(this,arguments)}t.prototype._init=function(t,e){var r=e.length||0;this.length=r;this._remaining=r;this._result=new Array(r);this._enumerate(e);0===this._remaining&&b(this.promise,this._result)};t.prototype._enumerate=function(t){for(var e=this.length,r=this.promise,n=0;r._state===At&&n<e;n++)this._eachEntry(t[n],n)};t.prototype._settleMaybeThenable=function(t,e){var r=this._instanceConstructor,n=r.resolve;if(n===f){var o=p(t);if(o===P&&t._state!==At){t._onError=null;this._settledAt(t._state,e,t._result)}else if("function"!=typeof o){this._remaining--;this._result[e]=this._makeResult(Pt,e,t)}else if(r===Nt){var i=new r(h);_(i,t,o);this._willSettleAt(i,e)}else this._willSettleAt(new r(function(e){return e(t)}),e)}else this._willSettleAt(n(t),e)};t.prototype._eachEntry=function(t,e){if(a(t))this._settleMaybeThenable(t,e);else{this._remaining--;this._result[e]=this._makeResult(Pt,e,t)}};t.prototype._settledAt=function(t,e,r){var n=this.promise;if(n._state===At)if(this._abortOnReject&&t===Rt)g(n,r);else{this._remaining--;this._result[e]=this._makeResult(t,e,r);0===this._remaining&&b(n,this._result)}};t.prototype._makeResult=function(t,e,r){return r};t.prototype._willSettleAt=function(t,e){var r=this;j(t,void 0,function(t){return r._settledAt(Pt,e,t)},function(t){return r._settledAt(Rt,e,t)})};return t}(),Ct="rsvp_"+Ot()+"-",It=0,Nt=function(){function t(e,r){this._id=It++;this._label=r;this._state=void 0;this._result=void 0;this._subscribers=[];jt.instrument&&c("created",this);if(h!==e){"function"!=typeof e&&C();this instanceof t?A(this,e):I()}}t.prototype._onError=function(t){var e=this;jt.after(function(){e._onError&&jt.trigger("error",t,e._label)})};t.prototype["catch"]=function(t,e){return this.then(void 0,t,e)};t.prototype["finally"]=function(t,e){var r=this,n=r.constructor;return r.then(function(e){return n.resolve(t()).then(function(){return e})},function(e){return n.resolve(t()).then(function(){throw e})},e)};return t}();Nt.cast=f;Nt.all=k;Nt.race=x;Nt.resolve=f;Nt.reject=M;Nt.prototype._guidKey=Ct;Nt.prototype.then=P;var Vt=new N,Dt=new N,Kt=function(t){function e(e,r,n){return $(this,t.call(this,e,r,!1,n))}z(e,t);return e}(Mt);Kt.prototype._makeResult=R;var Ut=Object.prototype.hasOwnProperty,qt=function(t){function e(e,r){var n=!(arguments.length>2&&void 0!==arguments[2])||arguments[2],o=arguments[3];return J(this,t.call(this,e,r,n,o))}Q(e,t);e.prototype._init=function(t,e){this._result={};this._enumerate(e);0===this._remaining&&b(this.promise,this._result)};e.prototype._enumerate=function(t){var e=this.promise,r=[];for(var n in t)Ut.call(t,n)&&r.push({position:n,entry:t[n]});var o=r.length;this._remaining=o;for(var i=void 0,u=0;e._state===At&&u<o;u++){i=r[u];this._eachEntry(i.entry,i.position)}};return e}(Mt),Ft=function(t){function e(e,r,n){return Z(this,t.call(this,e,r,!1,n))}tt(e,t);return e}(qt);Ft.prototype._makeResult=R;var Gt=0,Lt=void 0,Wt="undefined"!=typeof window?window:void 0,Yt=Wt||{},$t=Yt.MutationObserver||Yt.WebKitMutationObserver,zt="undefined"==typeof self&&"undefined"!=typeof process&&"[object process]"==={}.toString.call(process),Bt="undefined"!=typeof Uint8ClampedArray&&"undefined"!=typeof importScripts&&"undefined"!=typeof MessageChannel,Ht=new Array(1e3),Jt=void 0;Jt=zt?lt():$t?pt():Bt?yt():void 0===Wt&&"function"==typeof require?_t():vt();var Qt=void 0;if("object"==typeof self)Qt=self;else{if("object"!=typeof global)throw new Error("no global: `self` or `global` found");Qt=global}var Xt;jt.async=ft;jt.after=function(t){return setTimeout(t,0)};var Zt=it,te=function(t,e){return jt.async(t,e)};if("undefined"!=typeof window&&"object"==typeof window.__PROMISE_INSTRUMENTATION__){var ee=window.__PROMISE_INSTRUMENTATION__;n("instrument",!0);for(var re in ee)ee.hasOwnProperty(re)&&wt(re,ee[re])}var ne=(Xt={asap:ft,cast:Zt,Promise:Nt,EventTarget:gt,all:Y,allSettled:B,race:H,hash:X,hashSettled:et,rethrow:rt,defer:nt,denodeify:F,configure:n,on:wt,off:bt,resolve:it,reject:ut,map:ot},mt(Xt,"async",te),mt(Xt,"filter",ct),Xt);t["default"]=ne;t.asap=ft;t.cast=Zt;t.Promise=Nt;t.EventTarget=gt;t.all=Y;t.allSettled=B;t.race=H;t.hash=X;t.hashSettled=et;t.rethrow=rt;t.defer=nt;t.denodeify=F;t.configure=n;t.on=wt;t.off=bt;t.resolve=it;t.reject=ut;t.map=ot;t.async=te;t.filter=ct;Object.defineProperty(t,"__esModule",{value:!0})});
/*global GetGlobalContext, unescape, Xrm*/
/**
    Sonoma XRM Utility for Javascript.
    @module Sonoma
**/
/**
    @class Sonoma
**/
var Sonoma = Sonoma || (function () {

    var serverUrlRE = /^(?:http)(?:s)?\:\/\/([^\/]+)/i,
        webResourcesRE = /[^{]*/,
        endsWithSlashRE = /\/$/,
        aboutRE = /^about:/,
        getSingleAttribute,
        getSingleControl,
        classToTypeTable = {},
        types = 'Boolean Number String Function Array Date RegExp Object'.split(' '),
        i = 0,
        len = types.length;

    for (; i < len; i++) {
        classToTypeTable['[object ' + types[i] + ']'] = types[i].toLowerCase();
    }

    function extend(options) {
        for (var i in options) {
            if (options.hasOwnProperty(i)) {
                this[i] = options[i];
            }
        }
        return this;
    }

    function memoize(fn) {
        /*jslint vars: true, noarg: false*/
        var cache = {},
    
            memoizedFn = function () {
                // Copy the arguments object into an array: allows it to be used as
                // a cache key.
                var args = [];
                for (var i = 0; i < arguments.length; i++) {
                    args[i] = arguments[i];
                }
    
                // Evaluate the memoized function if it hasn't been evaluated with
                // these arguments before.
                if (!(args in cache)) {
                    cache[args] = fn.apply(this, arguments);
                }
    
                return cache[args];
            };
    
        return memoizedFn;
    }
    
    function memoized(fn,  key) {
        fn._values = fn._values || {};
        return fn._values[key] !== undefined ?
            fn._values[key] : fn._values[key] = fn.apply(fn, arguments);
    }
    
    /**
        Takes in an object of any type and returns its type as an all lowercase string.
        @method type
        @param obj {Object} An object of unknown type.
        @return {string} The type of the object as a lowercase string.
    **/
    function type(obj) {
        /// <summary>Takes in an object of any type and returns its type as an all lowercase string.</summary>
        /// <return>string</return>
        return obj === void(0) || obj === null ? // null or undefined
            String(obj) :
            classToTypeTable[Object.prototype.toString.call(obj)] || 'object';
    }

    /**
        Wrapper for Xrm.Page.context.client.getClient()
        @method getClient
        @return {string} Returns a value to indicate which client the script is executing in or an empty string if unable to determine.
    **/
    function getClient() {
        if (window.Xrm && Xrm.Page && Xrm.Page.context && Xrm.Page.context.client && Xrm.Page.context.client.getClient) {
            return Xrm.Page.context.client.getClient();
        } else if (window.GetGlobalContext) {
            var context = window.GetGlobalContext();
            if (context && context.client && context.client.getClient) {
                return context.client.getClient();
            }
        } else {
            return '';
        }
    }

    /**
        Gets the client URL.
        @method getClientUrl
        @return {string} The Client URL
    **/
    function getClientUrl() {
        /*jslint newcap: true*/
        var correctHost = window.location.host,
            xrmServerUrl = '',
            hasBadHost = true,
            hasBadProtocol = false,
            windowUrl,
            badHost,
            leftOfWebResource,
            context,
            isAbout;

        if (window.Xrm && Xrm.Page && Xrm.Page.context) {
            context = Xrm.Page.context;
            if (context.getClientUrl) {
                return context.getClientUrl();
            } else if (context.getServerUrl) {
                xrmServerUrl = context.getServerUrl();
            }
        }
        else if (window.GetGlobalContext) {
            context = window.GetGlobalContext();
            if (context.getClientUrl) {
                xrmServerUrl = context.getClientUrl();
            } else if (context.getServerUrl) {
                xrmServerUrl = context.getServerUrl();
            }
        }
        else {
            windowUrl = unescape(window.location.href).toLowerCase();
            if (windowUrl.indexOf('/webresources') !== -1) {
                leftOfWebResource = windowUrl.split('/webresources')[0];
                xrmServerUrl = leftOfWebResource.match(webResourcesRE)[0];

                hasBadHost = false;
            }
        }

        if (!xrmServerUrl) {
            alert('Unable to determine server url using Sonoma.getServerUrl.  Please include ClientGlobalContext.js.aspx.');
            return;
        }

        isAbout = window.location.protocol.match(aboutRE) !== null;
        hasBadProtocol = xrmServerUrl.indexOf(window.location.protocol) === -1;


        if (hasBadHost && !isAbout) {
            badHost = xrmServerUrl.match(serverUrlRE)[1];
            xrmServerUrl = xrmServerUrl.replace(badHost, correctHost);
        }

        if (hasBadProtocol && !isAbout) {
            xrmServerUrl = window.location.protocol + xrmServerUrl.substring(xrmServerUrl.indexOf(':') + 1);
        }

        if (xrmServerUrl.match(endsWithSlashRE)) {
            xrmServerUrl = xrmServerUrl.substring(0, xrmServerUrl.length - 1);
        }

        return xrmServerUrl;
    }

    /**
        Gets the server URL.
        @method getServerUrl
        @return {string} The Server URL
    **/
    function getServerUrl() {
        Sonoma.Log.warn([
            "Deprecation warning. 'Sonoma.getServerUrl' has been deprecated and will be removed in the next major release. ",
            "Please be sure to migrate existing code to use the new 'Sonoma.getClientUrl' function."
        ].join(''));
        return getClientUrl();
    }

    /**
        Gets the client URL with the organization name removed.
        @method getClientUrlWithoutOrg
        @return {string} The client URL without the organization name
    **/
    function getClientUrlWithoutOrg() {
        /* jshint newcap: false */
        var orgName = null,
            url;

        if (window.Xrm && Xrm.Page && Xrm.Page.context) {
            orgName = Xrm.Page.context.getOrgUniqueName();
        }
        else if (window.GetGlobalContext) {
            orgName = GetGlobalContext().getOrgUniqueName();
        }

        if (!orgName) {
            alert('Unable to determine the organization name using Sonoma.getServerUrlWithoutOrg.  Please include ClientGlobalContext.js.aspx.');
            return;
        }
        url = getClientUrl().replace(orgName, '');
        if (!url.match(endsWithSlashRE)) {
            url += '//';
        }
        return url;
    }

    function getServerUrlWithoutOrg() {
        Sonoma.Log.warn([
           "Deprecation warning. 'Sonoma.getServerUrlWithoutOrg' has been deprecated and will be removed in the next major release. ",
           "Please be sure to migrate existing code to use the new 'Sonoma.getClientUrlWithoutOrg' function."
        ].join(''));
        return getClientUrlWithoutOrg();
    }

    function AttributeList() {
        var that = this,
            supportedFunction;
        this.length = 0;

        supportedFunction = [
            'addOnChange', 'fireOnChange', 'getAttributeType',
            'getFormat', 'getInitialValue', 'getIsDirty', 'getMax', 'getMaxLength',
            'getMin', 'getName', 'getOption', 'getParent', 'getPrecision',
            'getRequiredLevel', 'getSelectedOption', 'getSubmitMode', 'getText',
            'getUserPrivilege', 'getValue', 'removeOnChange', 'setRequiredLevel',
            'setSubmitMode', 'setValue'
        ];

        each(supportedFunction, function (index, fnName) {
            that[fnName] = function () {
                /*jslint vars: true*/
                var args = arguments;
                var returnVal = null;

                // Use a for statement instead of each() because this is an array/object hybrid
                for (var i = 0; i < this.length; i++) {
                    var attribute = this[i];

                    var result = attribute[fnName].apply(attribute, args);
                    if (result && returnVal === null) {
                        returnVal = result;
                    }
                }

                return returnVal || that;
            };
        });
    }

    /**
        Returns the attributes on a CRM form.
        @method getAttribute
        @param [...args] {Array|Strings} Specific attributes you want to return. Omit to retrieve all attributes.
        @return {Array|Object} The desired attributes.
    **/
    function getAttribute(args) {
        var results = new AttributeList();
        if (Sonoma.type(args) !== 'array') {
            args = Array.prototype.slice(arguments, 0);
        }

        each(args, function (index, id) {
            var attribute = getSingleAttribute(id);
            if (attribute) {
                if (Sonoma.type(attribute) !== 'array') {
                    attribute = [attribute];
                }

                each(attribute, function () {
                    Array.prototype.push.call(results, this);
                });
            }
        });

        return results;
    }

    getSingleAttribute = memoize(function (attributeId) {
        if (window.Xrm && Xrm.Page && Xrm.Page.getAttribute) {
            return Xrm.Page.getAttribute(attributeId);
        }

        throw new Error('Cannot use getAttribute: Xrm.Page.getAttribute is not available.');
    });

    function ControlList() {
        var that = this,
            supportedFunction;
        this.length = 0;

        supportedFunction = [
            'addCustomView', 'addOption', 'clearOptions', 'getAttribute',
            'getControlType', 'getData', 'getDefaultView', 'getDisabled',
            'getLabel', 'getName', 'getParent', 'getSrc', 'getInitialUrl',
            'getObject', 'getVisible', 'refresh', 'removeOption', 'setData',
            'setDefaultView', 'setDisabled', 'setFocus', 'setLabel', 'setSrc',
            'setVisible'
        ];

        each(supportedFunction, function (index, fnName) {
            that[fnName] = function () {
                /*jslint vars: true*/
                var args = arguments;
                var returnVal = null;

                // Use a for statement instead of each() because this is an array/object hybrid
                for (var i = 0; i < this.length; i++) {
                    var control = this[i];

                    if (control[fnName]) {
                        var result = control[fnName].apply(control, args);
                        if (result && returnVal === null) {
                            returnVal = result;
                        }
                    }
                }

                return returnVal || that;
            };
        });
    }

    /**
        Returns the controls on a CRM form.
        @method getControl
        @param [...args] {Array|Strings} Specific controls you want to return. Omit to retrieve all controls.
        @return {Array/Object} The desired controls.
    **/
    function getControl(args) {
        var results = new ControlList();
        if (Sonoma.type(args) !== 'array') {
            args = Array.prototype.slice.call(arguments, 0);
        }

        each(args, function (index, id) {
            var control = getSingleControl(id);
            if (control) {
                if (Sonoma.type(control) !== 'array') {
                    control = [control];
                }

                each(control, function () {
                    Array.prototype.push.call(results, this);
                });
            }
        });

        return results;
    }

    getSingleControl = memoize(function getSingleControl(controlId) {
        if (window.Xrm && Xrm.Page && Xrm.Page.getControl) {
            return Xrm.Page.getControl(controlId);
        }

        throw 'Cannot use getControl: Xrm.Page.getControl is not available.';
    });

    /**
        Determines if two or more arguments are equivalent.
        @method areEqual
        @param ...args {Any} Parameters to test.
        @return {Boolean} True/False if the parameters are equivalent.
    **/
    function areEqual() {
        /// <summary>Takes in n parameters and returns if they are all equal using a strict comparison check.</summary>
        /// <returns>Boolean</return>
        var args = Array.prototype.slice.call(arguments, 0),
            lastArg;

        if (args.length === 0) {
            return true;
        }

        lastArg = args[0];

        return each(args, function (i, a) {
            /*jslint vars: true*/
            var eq = a === lastArg;

            lastArg = a;
            return eq;
        });
    }

    /**
        Returns the parameters in the query string.
        @method getQueryStringParams
        @param {String} [queryString] Parses the provided query string or window.location if null
        @return {Object} Object containing all parameters found in the query string as properties.
    **/
    function getQueryStringParams(queryString) {
        var searchString = (queryString || window.location.search || '').replace(/^\?/, ''),
            params = {},
            nameValuePairs = searchString.split('&'),
            nameValuePair,
            value,
            count = nameValuePairs.length;

        while (count--) {
            nameValuePair = nameValuePairs[count].split('=');
            if (nameValuePair.length === 2) {
                value = unescape(nameValuePair[1]);
                if (/(.+?)=/.test(value)) {
                    value = getQueryStringParams(value);
                }
                params[nameValuePair[0].toLowerCase()] = value;
            }
        }

        return params;
    }

    /**
        Execute a function for every element in an array or object.
        If the callback returns false, iteration stops.
        @method each
        @param obj {Array|Object} An array or object to iterate through.
        @param callback {Function} Function, receving arguments index and element, to execute for each element. Element receives context of "this" within the function.
        @return {Boolean} False if any iteration returns False, otherwise True.
        @example
            var names = ['Homer', 'Marge', 'Bart', 'Lisa'];
            Sonoma.each(names, function (index, name) {
                alert('Name #' + index + ' is ' + name);
            });
            // → 'Name #0 is Homer'
            // → 'Name #1 is Marge'
            // → 'Name #2 is Bart'
            // → 'Name #3 is Lisa'
   **/
    function each(obj, callback) {
        var i,
            p;

        if (Sonoma.type(callback) !== 'function') { return; }

        switch (Sonoma.type(obj)) {
            case 'array':
                for (i = 0; i < obj.length; i++) {
                    if (callback.call(obj[i], i, obj[i]) === false) {
                        return false;
                    }
                }

                return true;
            case 'object':
                for (p in obj) {
                    if (obj.hasOwnProperty(p)) {
                        if (callback.call(obj[p], p, obj[p]) === false) {
                            return false;
                        }
                    }
                }

                return true;
            default:
                throw new Error('Sonoma.each does not support the object ' + obj.toString());
        }
    }

    return {
        areEqual: areEqual,
        each: each,
        getClient: getClient,
        getClientUrl: getClientUrl,
        getServerUrl: getServerUrl,
        getServerUrlWithoutOrg: getServerUrlWithoutOrg,
        getQueryStringParams: getQueryStringParams,
        getAttribute: getAttribute,
        getControl: getControl,
        extend: extend,
        memoize: memoize,
        memoized: memoized,
        type: type,
        version: '@VERSION@'
    };

}());

/// <reference path="sonoma.js" />
/**
    Array Module
    @module Sonoma
    @submodule Array
**/
/**
    Cross-browser array utilities
    @class Array
    @namespace Sonoma
**/
Sonoma.Array = (function () {

    /**
        Gets the first index in an array containing the desired value.
        @method indexOf
        @param {Array} array An array to search
        @param {Object} value The value to search for
        @param {Number} [fromIndex=0] The index to begin searching from.
        @return {Number} The index for the value. Returns -1 if not found
    **/
    function indexOfPolyfill(array, value, fromIndex) {
        var index,
            length;

        if (!array || !isArrayPolyfill(array)) {
            return -1;
        }

        if (Array.prototype.indexOf) {
            return Array.prototype.indexOf.call(array, value, fromIndex);
        }

        index = (fromIndex || 0) - 1;
        length = array.length;

        while (++index < length) {
            if (array[index] === value) {
                return index;
            }
        }

        return -1;
    }

    /**
        Test if an object is an array
        @method isArray
        @param obj {Object} The object to test
        @return {boolean} True/False if the object is an array
    **/
    function isArrayPolyfill(obj) {
        if (Array.isArray) {
            return Array.isArray(obj);
        }

        return Object.prototype.toString.call(obj) === '[object Array]';
    }

    return {
        indexOf: indexOfPolyfill,
        isArray: isArrayPolyfill
    };

}());
/// <reference path="sonoma.js" />
/**
    String Module
    @module Sonoma
    @submodule String
**/
/**
    String Utilities
    @class String
    @namespace Sonoma
**/

Sonoma.String = (function () {
    var trimRE = /^\s+|\s+$/g;
    /**
        Format an `inputString` to include values.

        The `inputString` should contain `{0}`-`{n}`
        to be replaced by each value passed in as additional arguments.
        @method format
        @param inputString {String} String to be format
        @param [...values] {String}  Strings to be inserted into the `inputString`
        @return {String} Formatted String
        @example
            Sonoma.String.format('{0} {1}!', 'Hello', 'Sonoma');
            // → 'Hello Sonoma!'
    **/
    function format(inputString) {
        var i = 0,
            valueArray = [],
            len,
            matches,
            index;
        if (!inputString) { return ''; }
        valueArray = Array.prototype.slice.call(arguments, 1);
        matches = inputString.match(/\{\d+\}/g);
        if (matches) {
            for (len = matches.length; i < len; i++) {
                index = parseInt(matches[i].match(/\d+/), 10);
                if (valueArray.length > index) {
                    inputString = inputString.replace(matches[i], valueArray[index]);
                }
            }
        }
        return inputString;
    }
    /**
        Removes leading and trailing whitespace.
        @method trim
        @param inputString {String} String to trim
        @return {String} Trimmed String
        @example
            Sonoma.String.trim('  Hello Sonoma!  ');
            // → 'Hello Sonoma!'
    **/
    function trim(inputString) {
        return inputString.replace(trimRE, '');
    }
    /**
        Concatenate strings.
        @method concat
        @param ...inputStrings {String} Strings to concatenate
        @return {String} Concatenated strings
        @example
            Sonoma.String.concat('Hello', ' ', 'Sonoma', '!');
            // → 'Hello Sonoma!'
    **/
    function concat() {
        var args = Array.prototype.slice.call(arguments, 0);
        return args.join('');
    }
    /**
        Check if a string is `Null` or `''`.
        @method isNullOrEmpty
        @param inputString {String} String to check
        @return {Boolean} _True_ if _null_ or Empty. _False_ otherwise.
        @example
            Sonoma.String.isNullOrEmpty('Hello');
            // → false
            Sonoma.String.isNullOrEmpty('');
            // → true
            Sonoma.String.isNullOrEmpty(null);
            // → true
    **/
    function isNullOrEmpty(inputString) {
        return inputString === '' || inputString === undefined || inputString === null || typeof inputString !== "string";
    }
    /**
        Pad a string on the left.
        @method padLeft
        @param inputString {String}  String to pad
        @param paddingCharacter {String} String to use for padding
        @param paddingCount {number} How many times to add `paddingCharacter`
        @return {String} Left-padded string
        @example
            Sonoma.String.padLeft('Hello Sonoma!', '0', 3);
            // → '000Hello Sonoma!'
    **/
    function padLeft(inputString, paddingCharacter, paddingCount) {
        var resultArray = [];

        while (paddingCount-- > 0) {
            resultArray.push(paddingCharacter);
        }

        resultArray.push(inputString);
        return resultArray.join('');
    }
    /**
        Pad a string on the right.
        @method padRight
        @param inputString {String} String to pad
        @param paddingCharacter {String} String to use for padding
        @param paddingCount {number} How many times to add `paddingCharacter`
        @return {String} Right-padded string
        @example
            Sonoma.String.padRight('Hello Sonoma!', '0', 3);
            // → 'Hello Sonoma!000'
    **/
    function padRight(inputString, paddingCharacter, paddingCount) {
        var resultArray = [inputString];

        while (paddingCount-- > 0) {
            resultArray.push(paddingCharacter);
        }

        return resultArray.join('');
    }
    return {
        format: format,
        trim: trim,
        concat: concat,
        isNullOrEmpty: isNullOrEmpty,
        padLeft: padLeft,
        padRight: padRight
    };
}());

/// <reference path="sonoma.js" />
/**
    Date Module
    @module Sonoma
    @submodule Date
**/
/**
    Date Utilities

    A _Date Representation_ is a _Date_, _Array_, _Number_, _String_, or _Object_.
      
    * _Date_
        * A JavaScript _Date_
    * _Array_
        * [Number of year, Number of month, Number of day]
    * _Number_
        * Milliseconds since Unix Epoch
    * _String_
        * An [_ISO 8601_](http://tools.ietf.org/html/rfc2822#page-14) formatted string
    * _Object_
        * {year: _Number_, month: _Number_, date: _Number_}

    @class Date
    @namespace Sonoma
**/
/*globals Sys*/
Sonoma.Date = (function () {
    var dateMetadata = {
        ShortDatePattern: 'M/d/yyyy',
        ShortTimePattern: 'h:mm tt',
        AMDesignator: 'AM',
        PMDesignator: 'PM'
    },
    //One Valid Date Format: YYYY-MM-DD HH:mm:ss.SSSZZ
    dateRegex = /(\d{4})-?(\d{2})-?(\d{2})(?:[T ](\d{2}):?(\d{2}):?(\d{2})?(?:\.(\d{1,3})[\d]*)?(?:(Z)|([+\-])(\d{2})(?::?(\d{2}))?))/;
    

    try {
        // Try to get the date metadata info from CRM.
        dateMetadata = Sys.CultureInfo.CurrentCulture.dateTimeFormat;
    }
    catch (e) { }

    function _formatDate(d, pattern) {
        var fd = d.toString(),
            month,
            day,
            militaryHour,
            shortHour,
            minutes,
            seconds;

        fd = pattern.replace(/yyyy/g, d.getFullYear());
        fd = fd.replace(/yy/g, (d.getFullYear() + '').substring(2));

        month = d.getMonth();
        fd = fd.replace(/MM/g, month + 1 < 10 ? '0' + (month + 1) : month + 1);
        fd = fd.replace(/(\\)?M/g, function ($0, $1) { return $1 ? $0 : month + 1; });

        day = d.getDate();
        fd = fd.replace(/dd/g, day < 10 ? '0' + day : day);
        fd = fd.replace(/(\\)?d/g, function ($0, $1) { return $1 ? $0 : day; });

        militaryHour = d.getHours();
        shortHour = militaryHour > 12 ? militaryHour - 12 : militaryHour;

        fd = fd.replace(/HH/g, militaryHour < 10 ? '0' + militaryHour : militaryHour);
        fd = fd.replace(/(\\)?H/g, function ($0, $1) { return $1 ? $0 : militaryHour; });

        fd = fd.replace(/hh/g, militaryHour < 10 ? '0' + shortHour : shortHour);
        fd = fd.replace(/(\\)?h/g, function ($0, $1) { return $1 ? $0 : shortHour; });

        minutes = d.getMinutes();
        fd = fd.replace(/mm/g, minutes < 10 ? '0' + minutes : minutes);
        fd = fd.replace(/(\\)?m/g, function ($0, $1) { return $1 ? $0 : minutes; });

        seconds = d.getSeconds();
        fd = fd.replace(/ss/g, seconds < 10 ? '0' + seconds : seconds);
        fd = fd.replace(/(\\)?s/g, function ($0, $1) { return $1 ? $0 : seconds; });

        fd = fd.replace(/fff/g, d.getMilliseconds());

        fd = fd.replace(/tt/g, d.getHours() >= 12 ? dateMetadata.PMDesignator : dateMetadata.AMDesignator);

        return fd.replace(/\\/g, '');
    }

    /**
        Returns the number of milliseconds between January 1, 1970 and the provided date (UTC Time).
        @method parse
        @param {String} date String representing a date formatted as "YYYY-MM-DD HH:mm:ss.SSSZZ"
        @return {Number} Number of milliseconds between January 1, 1970 and the provided date
        @example
            var seconds = Sonoma.Date.parse('2017-1-1')
            // seconds → 1483250400000
    **/
    function parse(d) {
        var timestamp = Date.parse(d),
            minutesOffset = 0,
            dateMatches,
            year,
            month,
            date,
            hours,
            minutes,
            seconds,
            milliseconds,
            utcOffsetMinutes,
            offsetHours,
            offsetMinutes;
        dateMatches = dateRegex.exec(d);
        if (isNaN(timestamp) && dateMatches) {
            year = parseInt(dateMatches[1], 10) || 0;
            month = (parseInt(dateMatches[2], 10) || 0) - 1;
            date = parseInt(dateMatches[3], 10) || 0;
            hours = parseInt(dateMatches[4], 10) || 0;
            minutes = parseInt(dateMatches[5], 10) || 0;
            seconds = parseInt(dateMatches[6], 10) || 0;
            milliseconds = parseInt(dateMatches[7], 10) || 0;
            offsetHours = (parseInt(dateMatches[10], 10) || 0);
            offsetMinutes = (parseInt(dateMatches[11], 10) || 0);
            if (dateMatches[8] !== 'Z') {
                utcOffsetMinutes = offsetHours * 60 + offsetMinutes;
                if (dateMatches[9] === '+') {
                    minutesOffset = 0 - minutesOffset;
                }
            }
            timestamp = Date.UTC(
                year, month, date,
                hours, minutes + utcOffsetMinutes, seconds,
                milliseconds);
        }
        return timestamp;
    }

    /**
        Returns a Date object as a String, using the ISO standard. 
        @method toISOString
        @param {Date} date Date object
        @return {String} String representation of date formatted to ISO standard
        @example
            var d = Sonoma.Date.convert(Date.now());
            alert('The current time is ' + Sonoma.Date.toISOString(d))
            // → 'The current time is 2017-01-18T18:58:15.977Z'
    **/
    function toISOString(d) {
        var month,
            date,
            hours,
            minutes,
            seconds,
            milliseconds;

        if (!(d instanceof Date)) {
            Sonoma.Log.error('Error in Sonoma.Date.toISOString: Object of type Date was expected.');
            return;
        }

        month = d.getUTCMonth() + 1;
        if (month.toString().length === 1) {
            month = Sonoma.String.padLeft(month, '0', 1);
        }
        date = d.getUTCDate();
        if (date.toString().length === 1) {
            date = Sonoma.String.padLeft(date, '0', 1);
        }
        hours = d.getUTCHours();
        if (hours.toString().length === 1) {
            hours = Sonoma.String.padLeft(hours, '0', 1);
        }
        minutes = d.getUTCMinutes();
        if (minutes.toString().length === 1) {
            minutes = Sonoma.String.padLeft(minutes, '0', 1);
        }
        seconds = d.getUTCSeconds();
        if (seconds.toString().length === 1) {
            seconds = Sonoma.String.padLeft(seconds, '0', 1);
        }
        milliseconds = d.getUTCMilliseconds();
        if (milliseconds.toString().length === 1) {
            milliseconds = Sonoma.String.padLeft(milliseconds, '0', 2);
        }
        else if (milliseconds.toString().length === 2) {
            milliseconds = Sonoma.String.padLeft(milliseconds, '0', 1);
        }

        return d.getUTCFullYear() + '-' + month + '-' + date + 'T' +
            hours + ':' + minutes + ':' + seconds + '.' + milliseconds + 'Z';
    }

    /**
        Converts the input into a valid _Date_ object. 
        @method convert
        @param {Array|Date|Number|Object|String} date Anything that represents a date.
        @param {Number} [date.year] If date is an Object, the date's year as a number
        @param {Number} [date.month] If date is an Object, the date's month as a number
        @param {Number} [date.date] If date is an Object, the date's day as a number
        @return {Date|NaN} A Date object if input can be converted, otherwise NaN
    **/
    function convert(input) {
        switch (Sonoma.type(input)) {
            case 'date':
                return input;
            case 'array':
                return new Date(input[0], input[1], input[2]);
            case 'number':
                return new Date(input);
            case 'string':
                return new Date(parse(input));
            case 'object':
                if (input.year !== undefined && input.month !== undefined && input.date !== undefined) {
                    return new Date(input.year, input.month, input.date);
                }
        }

        return NaN;
    }

    /**
        Determines if a date is within a specified range. 
        @method inRange
        @param {Array|Date|Number|Object|String} date The date to check
        @param {Array|Date|Number|Object|String} start Start date of the range
        @param {Array|Date|Number|Object|String} end End date of the range
        @return {Boolean} True/False if the date is between the start and end dates.
    **/
    function inRange(d, start, end) {
        return (
            isFinite(d = convert(d).valueOf()) &&
            isFinite(start = convert(start).valueOf()) &&
            isFinite(end = convert(end).valueOf()) ?
            start <= d && d <= end :
            NaN);
    }

    /**
        Creates a Date object from the specified date, with the time set to 0:00:000 (Midnight).
        @method zeroTime
        @param {Array|Date|Number|Object|String} input The date to modify
        @param {Number} [input.year] If input is an Object, the date's year as a number
        @param {Number} [input.month] If input is an Object, the date's month as a number
        @param {Number} [input.date] If input is an Object, the date's day as a number
        @return {Date} Modified date
    **/
    function zeroTime(input) {
        var date = convert(input);
        date.setHours(0);
        date.setMinutes(0);
        date.setSeconds(0);
        date.setMilliseconds(0);
        return date;
    }

    /**
        Creates a string for the date in "M/D/YYYY" format.
        @method getShortDateFormat
        @param {Array|Date|Number|Object|String} input The date to format
        @param {Number} [input.year] If input is an Object, the date's year as a number
        @param {Number} [input.month] If input is an Object, the date's month as a number
        @param {Number} [input.date] If input is an Object, the date's day as a number
        @return {String} Formatted date
    **/
    function getShortDateFormat(input) {
        var date = convert(input);
        return _formatDate(date, dateMetadata.ShortDatePattern);
    }

    /**
        Creates a string for the time in "h:mm tt" format.
        @method getShortDateTimeFormat
        @param {Array|Date|Number|Object|String} input The date/time to format
        @return {String} Formatted time
        @example
            alert('The time is ' + Sonoma.Date.getShortDateTimeFormat(Date.now()))
            // → 'The time is 1:45 PM'
    **/
    function getShortDateTimeFormat(input) {
        var date = convert(input);
        return _formatDate(date, dateMetadata.ShortTimePattern);
    }

    return {
        parse: parse,
        toISOString: toISOString,
        convert: convert,
        inRange: inRange,
        zeroTime: zeroTime,
        getShortDateFormat: getShortDateFormat,
        getShortDateTimeFormat: getShortDateTimeFormat
    };

}());

/// <reference path="sonoma.js" />
/// <reference path="core.js" />
/*globals Sonoma XMLDocument Element window XPathResult*/
/**
    Guid Module
    @module Sonoma
    @submodule Guid
**/
/**
    Guid Utilities
    @class Guid
    @namespace Sonoma
**/
Sonoma.Guid = (function () {
    var bracketsRE = /[{}]/g,
        normalizedGuidRE = /^([0-9A-F]{32})$/,
        formatGuidRE = /^([0-9A-F]{8})([0-9A-F]{4})([0-9A-F]{4})([0-9A-F]{4})([0-9A-F]{12})$/,
        normalizeRE = /[\s{}\-]/g,
        emptyGuidN = '00000000000000000000000000000000',
        emptyGuidD = '00000000-0000-0000-0000-000000000000',
        emptyGuidB = '{00000000000000000000000000000000}',
        emptyGuidDB = '{00000000-0000-0000-0000-000000000000}';

    /**
        Removes encapsulating brackets from Guid
        @method decapsulate
        @param guid {string} String representing a Guid
        @return {string} Guid without encapsulating brackets
        @example
            Sonoma.Guid.decapsulate('{00000000-0000-0000-0000-000000000000}');
            // → '00000000-0000-0000-0000-000000000000'
    **/
    function decapsulate(guid) {
        return guid.replace(bracketsRE, '');
    }

    /**
        Adds encapsulating brackets to Guid
        @method encapsulate
        @param guid {string} String representing a Guid
        @return {string} Guid with encapsulating brackets
        @example
            Sonoma.Guid.encapsulate('00000000-0000-0000-0000-000000000000');
            // → '{00000000-0000-0000-0000-000000000000}'
    **/
    function encapsulate(guid) {
        return '{' + decapsulate(guid) + '}';
    }

    /**
        Determines if Guid has a valid format
        @method isValid
        @param guid {string} String representing a Guid
        @return {boolean} True/False if Guid is valid
        @example
            Sonoma.Guid.isValid('00000000-0000-0000-0000-000000000000');
            // → true
            Sonoma.Guid.isValid('0000-0000-0000-0000-0000');
            // → false
    **/
    function isValid(guid) {
        if (Sonoma.type(guid) !== 'string') {
            return false;
        }
        guid = _normalizeGuid(guid);
        return normalizedGuidRE.test(guid);
    }

    /**
        Formats Guid string to be of the form '{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}'
        @method format
        @param guid {string} String representing a Guid
        @return {string} Formatted Guid
        @example
            Sonoma.Guid.format('00000000000000000000000000000000');
            // → '{00000000-0000-0000-0000-000000000000}'
    **/
    function format(guid) {
        if (!isValid(guid)) {
            return null;
        }
        guid = _normalizeGuid(guid);
        var matches = formatGuidRE.exec(guid);
        guid = Sonoma.String.format(
            "{{0}-{1}-{2}-{3}-{4}}",
            matches[1], matches[2],matches[3],matches[4],matches[5]);
        return guid;
    }

    function _hasValidBrackets(guid) {
        return (guid.indexOf('{') === -1 && guid.indexOf('}') === -1) ||
               _hasBrackets(guid);
    }

    function _hasBrackets(guid) {
        return (guid.indexOf('{') === 0 && guid.indexOf('}') === guid.length - 1);
    }

    function _normalizeGuid(guid) {
        if (Sonoma.type(guid) !== 'string') {
            return null;
        }
        return guid.replace(normalizeRE, '').toUpperCase();
    }

    /**
        Tests if 2 or more guids are equivalent, regardless of formatting
        @method areEqual
        @param guid* {string} String representing a Guid
        @return {boolean} True/False if all provided Guid's are equivalent
        @example
            Sonoma.Guid.areEqual('00000000000000000000000000000000', '{00000000-0000-0000-0000-000000000000}');
            // → true
    **/
    function areEqual() {
        var normalizedGuids = [],
            args = Array.prototype.slice.call(arguments, 0);
        Sonoma.each(args, function () {
            normalizedGuids.push(_normalizeGuid(this));
        });
        return Sonoma.areEqual.apply(null, normalizedGuids);
    }

    /**
        Returns a string representing an empty Guid
        @method empty
        @since 2.0
        @param [flag='db'] {string} Flag to indicate desired formatting of Guid.
        | Flag Options | Meaning |
        | ------------ | ------- |
        | 'n' | No formatting |
        | 'd' | Dashes only |
        | 'b' | Brackets only |
        | 'db' | Dashes and brackets |
        @return {boolean} True/False if all provided Guid's are equivalent
        @example
            Sonoma.Guid.empty();
            // → '{00000000-0000-0000-0000-000000000000}'
    **/
    function empty(flag) {
        flag = flag || 'db';

        switch (flag.toLowerCase()) {
            case 'n':
                return emptyGuidN;
            case 'b':
                return emptyGuidB;
            case 'db':
            case 'bd':
                return emptyGuidDB;
            case 'd':
                return emptyGuidD;
            default:
                return emptyGuidDB;
        }
    }

    /**
        Tests if a Guid is equivalent to Guid.Empty, regardless of formatting.
        @method isEmpty
        @since 2.0
        @param guid {string} String representing a Guid
        @return {boolean} True/False if the Guid is empty
        @example
            Sonoma.Guid.isEmpty('{00000000-0000-0000-0000-000000000000}');
            // → true

            Sonoma.Guid.isEmpty('00000000000000000000000000000000');
            // → true

            Sonoma.Guid.isEmpty('28b23b1c-a2f2-4a9e-8344-9f35377eb749');
            // → false
    **/
    function isEmpty(guid) {
        return areEqual(guid, empty());
    }

    return {
        decapsulate: decapsulate,
        encapsulate: encapsulate,
        isValid: isValid,
        isEmpty: isEmpty,
        format: format,
        areEqual: areEqual,
        empty: empty
    };

}());

/// <reference path="sonoma.js" />
/**
    Cache Module
    @module Sonoma
    @submodule Cache
**/
/**
    A key-value data store that resides in memory. Multiple sets of data can be organized in seperate cache objects (like a namespace).
    ```
    Sonoma.Cache.set('configs', 'config1', {});
    Sonoma.Cache.set('userData', 'role', 'System Administrator');

    Sonoma.Cache.get('userData', 'role');
    // → 'System Administrator'
    Sonoma.Cache.get('configs', 'config1');
    // → {}
    Sonoma.Cache.get('userData', 'config1'); // 'config1' key is only defined in 'configs' cache
    // → undefined
    ```
    @class Cache
    @namespace Sonoma
**/
Sonoma.Cache = (function () {
    var caches = [];

    function _getTargetFromCaches(cacheName) {
        var i = 0,
            cacheLength,
            newCacheObject;

        for (cacheLength = caches.length; i < cacheLength; i++) {
            if (caches[i].name === cacheName) {
                return caches[i];
            }
        }

        // If we reach this point, the target is not in the cache set, so create it and return it
        newCacheObject = { name: cacheName, data: [] };
        caches.push(newCacheObject);

        return newCacheObject;
    }

    /**
        Retrieve data from the cache.
        @method get
        @param {string} cacheName Name of the cache storing the data.
        @param {string} key Name of the key holding the value.
        @return {Object} The object stored in the cache for _key_, or `null` if the key is not found.
    **/
    function getData(cacheName, key) {
        var targetCache = _getTargetFromCaches(cacheName),
            i = 0,
            cacheDataLength;

        for (cacheDataLength = targetCache.data.length; i < cacheDataLength; i++) {
            if (targetCache.data[i].key === key) {
                return targetCache.data[i].value;
            }
        }

        return null;
    }

    /**
        Store data into the cache.
        @method set
        @param cacheName {string} Name of the cache to store the data.
        @param key {string} Name of the key to hold the value.
        @param value {Object} Value to be stored.
    **/
    function setData(cacheName, key, value) {
        var targetCache = _getTargetFromCaches(cacheName),
            i = 0,
            cacheDataLength;

        for (cacheDataLength = targetCache.data.length; i < cacheDataLength; i++) {

            if (targetCache.data[i].key === key) {
                targetCache.data[i].value = value;
                return;
            }
        }

        targetCache.data.push({ key: key, value: value });
    }

    return {
        get: getData,
        set: setData
    };

}());
/// <reference path="sonoma.js" />
/**
    LocalStorage Module
    @module Sonoma
    @submodule LocalStorage
**/
/**
    Stores key-value pairs using the browser's local storage, with an optional expiration time.
    @class LocalStorage
    @namespace Sonoma
**/

Sonoma.LocalStorage = (function () {

    var ticksPerHours = 3600000;

    if (!window.localStorage) {
        window.localStorage = {
            getItem: function () {
                return null;
            },
            setItem: function () {

            },
            removeItem: function () {

            }
        };
    }

    /**
        Retrieve a value from local storage using the provided key.
        @method get
        @param key {string} The key for the data to retrieve.
        @return {Object} The value retrieved from local storage.
    **/
    function get(key) {
        var returnValue = null,
            existingItem = window.localStorage.getItem(key),
            expirationDate;

        if (existingItem) {
            existingItem = JSON.parse(existingItem);
            expirationDate = existingItem.expiration || 0;
            if (expirationDate && expirationDate <= new Date().valueOf()) {
                // If the expiration date is in the past, clear the value and reset the expiration
                window.localStorage.removeItem(key);
            }
            else {
                returnValue = existingItem.value;
            }
        }

        return returnValue;
    }

    /**
        Store a key-value pair in local storage, with an optional expiration time.
        @method set
        @param key {string} Key of the item to save.
        @param value {object} The data to save.
        @param [expirationInHours] {Number} The number of hours the data is valid for. 
    **/
    function set(key, value, expirationInHours) {
        var expirationDate;

        if (value === null) {
            window.localStorage.removeItem(key);
        }
        else {
            value = { value: value, expiration: null };

            if (expirationInHours) {
                expirationDate = new Date(new Date().valueOf() + (ticksPerHours * expirationInHours));
                value.expiration = expirationDate.valueOf();
            }

            window.localStorage.setItem(key, JSON.stringify(value));
        }
    }


    /**
        Remove a key-value pair from local storage
        @method remove
        @param key {String} Key of the item to remove.
    **/
    function remove(key){
        window.localStorage.removeItem(key);
    }

    return {
        set: set,
        get: get,
        remove: remove
    };

}());
/// <reference path="sonoma.js" />
/**
    Log Module
    @module Sonoma
    @submodule Log
**/
/**
    Logging Utilities

    This class offers wrapped console functions with some additional functions.

    Further reference on MDN's [console](https://developer.mozilla.org/en-US/docs/Web/API/console) page.
    
    Using this will not crash an instance of IE with no console open.
    @class Log
    @namespace Sonoma
**/

Sonoma.Log = (function () {
    var logList = [],
        timers = {};

    /**
        Logs to the console.

        [console.log](https://developer.mozilla.org/en-US/docs/Web/API/console.log) on MDN
        @method log
        @param ...message {any} Message to log
    **/
    function log() {
        if (window.console && console.log) {
            Function.prototype.apply.call(console.log, console, arguments);
        }
        logList.push.apply(logList, arguments);
    }
    /**
        Logs info to the console.

        [console.info](https://developer.mozilla.org/en-US/docs/Web/API/console.info) on MDN
        @method info
        @since 2.0
        @param ...message {any} Message to log
    **/
    function info() {
        if (window.console && console.info) {
            Function.prototype.apply.call(console.info, console, arguments);
        } else if (window.console && console.log) {
            Function.prototype.apply.call(console.log, console, arguments);
        }
        logList.push.apply(logList, arguments);
    }
    /**
        Logs a warning to the console.

        [console.warn](https://developer.mozilla.org/en-US/docs/Web/API/console.warn) on MDN
        @method warn
        @since 2.0
        @param ...message {any} Message to log
    **/
    function warn() {
        if (window.console && console.warn) {
            Function.prototype.apply.call(console.warn, console, arguments);
        } else if (window.console && console.log) {
            Function.prototype.apply.call(console.log, console, arguments);
        }
        logList.push.apply(logList, arguments);
    }
    /**
        Logs an error to the console.

        [console.error](https://developer.mozilla.org/en-US/docs/Web/API/console.error) on MDN
        @method error
        @since 2.0
        @param ...message {any} Message to log
    **/
    function error() {
        if (window.console && console.error) {
            Function.prototype.apply.call(console.error, console, arguments);
        } else if (window.console && console.log) {
            Function.prototype.apply.call(console.log, console, arguments);
        }
        logList.push.apply(logList, arguments);
    }
    /**
        Gets all messages that have been logged.
        @method getLogs
        @since 2.0
        @return {Array} All logs.
    **/
    function getLogs() {
        return logList;
    }
    /**
        Clears all logs stored internally.
        @method clearLogs
        @since 2.0
    **/
    function clearLogs() {
        logList = [];
    }
    /**
        Creates a log group in the browser's console.

        [console.group](https://developer.mozilla.org/en-US/docs/Web/API/console.group) on MDN
        @method group
        @since 2.0
    **/
    function group() {
        if (window.console && console.group) {
            console.group();
        } else if (window.console && console.log) {
            console.log('+++');
        }
    }
    /**
        Removes a log group in the browser's console.

        [console.groupEnd](https://developer.mozilla.org/en-US/docs/Web/API/console.groupEnd) on MDN
        @method groupEnd
        @since 2.0
    **/
    function groupEnd() {
        if (window.console && console.groupEnd) {
            console.groupEnd();
        } else if (window.console && console.log) {
            console.log('---');
        }
    }
    /**
        Adds a collapsed log group in the browser's console.

        [console.groupCollapsed](https://developer.mozilla.org/en-US/docs/Web/API/console.groupCollapsed) on MDN
        @method groupCollapsed
        @since 2.0
    **/
    function groupCollapsed() {
        if (window.console && console.groupCollapsed) {
            console.groupCollapsed();
        } else {
            group();
        }
    }
    /**
        Starts a named timer.

        [console.time](https://developer.mozilla.org/en-US/docs/Web/API/console.time) on MDN
        @method time
        @since 2.0
        @param [timerName='default'] {String} Name of the timer.
    **/
    function time(timerName) {
        if (!timerName) {
            timerName = 'default';
        }

        if (window.console && console.time) {
            console.time(timerName);
        }
        else {
            timers[timerName] = currentTime();
        }
    }
    /**
        Ends a timer. Logs the milliseconds elapsed to the console.

        [console.timeEnd](https://developer.mozilla.org/en-US/docs/Web/API/console.timeEnd) on MDN
        @method timeEnd
        @since 2.0
        @param [timerName='default'] {String} Name of the timer.
    **/
    function timeEnd(timerName) {
        if (!timerName) {
            timerName = 'default';
        }

        if (window.console && console.timeEnd) {
            console.timeEnd(timerName);
        }
        else if (timers[timerName]) {
            log(timerName + ': ' + (currentTime() - timers[timerName]) + 'ms');
            delete timers[timerName];
        }
    }

    function currentTime() {
        // Date.now() does not exist in IE8
        return Date.now ? Date.now() : new Date().getTime();
    }

    /**
        Logs an interactive object.

        [console.dir](https://developer.mozilla.org/en-US/docs/Web/API/console.dir) on MDN
        @method dir
        @since 2.0
        @param object {Object} Object to log.
    **/
    function dir(object) {
        if (window.console && console.dir) {
            console.dir(object);
        } else if (window.console && console.log && JSON && JSON.stringify) {
            console.log(JSON.stringify(object));
        }
    }
    /**
        Logs a the current stacktrace.

        [console.trace](https://developer.mozilla.org/en-US/docs/Web/API/console.trace) on MDN
        @method trace
        @since 2.0
    **/
    function trace() {
        if (window.console && console.trace) {
            console.trace();
        } else if (window.console && console.error) {
            console.error('Stracktrace');
        } else if (window.console && console.log) {
            //TODO: stacktrace for IE7 and less
            console.log('console.trace not available on this browser.');
        }
    }
    return {
        log: log,
        debug: log, //alias
        info: info,
        warn: warn,
        error: error,
        getLogs: getLogs,
        clearLogs: clearLogs,
        group: group,
        groupEnd: groupEnd,
        groupCollapsed: groupCollapsed,
        time: time,
        timeEnd: timeEnd,
        dir: dir,
        trace: trace
    };
}());
/// <reference path='sonoma.js' />
/// <reference path='string.js' />
/*globals Sonoma XMLDocument Element window XPathResult*/
/**
    Xml Module
    @module Sonoma
    @submodule Xml
**/
/**
    Xml Utilities
    @class Xml
    @namespace Sonoma
**/
Sonoma.Xml = (function (undefined) {
    'use strict';

    /**
        Encodes a string to be used in XML
        @method xmlEncode
        @since 2.0
        @param XML {string} String with unescaped characters
        @return {string} XML Escaped String
    **/
    function xmlEncode(xml) {
        if (Sonoma.type(xml) === 'array') {
            xml = xml.join('');
        }
        if (Sonoma.type(xml) !== 'string') {
            xml = xml.toString();
        }
        return xml
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /**
        Parse an XML *string* to a *Document*

        @method loadXml
        @returns {Document}
    **/
    function loadXml(data) {
        var xml;
        if (!data || typeof data !== 'string') {
            return null;
        }
        // Support: IE 9 - 11 only
        // IE throws on parseFromString with invalid input.
        try {
            xml = (new window.DOMParser()).parseFromString(data, 'text/xml');
        } catch (e) {
            xml = undefined;
        }

        if (!xml || xml.getElementsByTagName('parsererror').length) {
            Sonoma.Log.error('Invalid XML: ' + data);
        }
        return xml;
    }

    //#region XMLObject
    /**
        Create an *XmlObject*
        @class XmlObject
        @since 2.0
        @namespace Sonoma.Xml
        @constructor
        @param tagName {string}
        @chainable
        @example
            var fetchXml = new Sonoma.Xml.XmlObject('fetch');
            fetchXml.addAttribute('mapping', 'logical');
            fetchXml.toString();
            // → '<fetch mapping='logical' />'
    **/
    function XmlObject(tagName) {
        if (!(this instanceof XmlObject)) {
            return new XmlObject(tagName);
        }
        if (typeof tagName === 'undefined') {
            throw new Error('tagName is required.');
        }
        this.tagName = tagName;
        this.attributes = {};
        this.elements = [];
    }
    XmlObject.prototype = (function () {
        /*jshint validthis:true */
        /**
            Adds an attribute to an *XmlObject*
            @method addAttribute
            @since 2.0
            @param key {String}
            @param value {Object}
            @chainable
        **/
        function addAttribute(key, value) {
            if (typeof key === 'undefined') {
                throw new Error('key is required.');
            }
            this.attributes[key] = xmlEncode(value.toString());
        }
        /**
            Adds an element to an *XmlObject*
            @method addElement
            @since 2.0
            @param element {Object} The element to add as a child
            @chainable
        **/
        function addElement(element) {
            if (typeof element === 'undefined') {
                throw new Error('element is required.');
            }
            if (element instanceof Array) {
                for (var i = 0, len = element.length; i < len; ++i) {
                    this.elements.push(element[i]);
                }
            }
            else {
                this.elements.push(element);
            }
        }
        /**
            Return lines indented
            @method nTabs
            @since 2.0
            @private
            @param tabDepth {number} Size left in tabbing the line.
        **/
        function nTabs(depth) {
            var tabDepth = [];
            while (depth--) {
                tabDepth.push('\t');
            }
            return tabDepth.join('');
        }
        /**
            Create a *string* of XML
            @method toString
            @since 2.0
            @param [PrettyPrint=false] {Boolean} Create a string with human readable XML.
            @param [Depth=0] {Number} Tab Depth
            @return {String} XML
        **/
        function toString(isPretty, depth) {
            var pretty = typeof isPretty !== 'undefined' ? isPretty : false,
                tabs,
                xmlString = [],
                attr, i, len, element;

            depth = depth || 0;

            tabs = nTabs(depth++);

            if (pretty) {
                xmlString.push(tabs);
            }
            xmlString.push('<' + this.tagName);
            //attributes
            for (attr in this.attributes) {
                if (this.attributes.hasOwnProperty(attr)) {
                    xmlString.push(' ' + attr + '=\'' + this.attributes[attr] + '\'');
                }
            }
            if (this.elements.length === 0) {
                xmlString.push(' />');
            }
            else {
                //element with content
                xmlString.push('>');
                for (i = 0, len = this.elements.length; i < len; ++i) {
                    element = this.elements[i];
                    if (element instanceof XmlObject) {
                        if (pretty) {
                            xmlString.push('\n');
                        }
                        xmlString.push(element.toString(pretty, depth));
                    }
                    else {
                        if (pretty) {
                            xmlString.push('\n' + nTabs(depth));
                        }
                        xmlString.push(xmlEncode(element.toString()));
                    }
                }
                if (pretty) {
                    xmlString.push('\n' + tabs);
                }
                xmlString.push('</' + this.tagName + '>');
            }
            return xmlString.join('');
        }
        return {
            addAttribute: addAttribute,
            addElement: addElement,
            toString: toString
        };
    })();
    //#endregion

    return {
        XmlObject: XmlObject,
        xmlEncode: xmlEncode,
        loadXml: loadXml
    };
}());

/// <reference path="../lib/rsvp.min.js" />
/// <reference path='sonoma.js' />

/**
    <h2>Promise Module</h2>

    Example of consuming promises:

    ```
    promise.then(function(value) {
        // do somthing with result
        // - or -
        // return result for chaining
    }).then(function(result) {
        // avoids nested callbacks! cool huh?
    }).catch(function(reason) {
        // handle error
    }).finally(function(){
        // perform logic regardless of success or failure
    });
    ```

    Example of creating promises:

    ```
    function exampleFn() {
        var promise = new Sonoma.Promise.Promise(function(resolve, reject) {
            setTimeout(function() {
                if (true) {
                    resolve('woohoo!');
                } else {
                    reject('wahwah');
                }
            }, 1000);
        });
        return promise;
    }
    ```
    ```
    function doSomethingAsync(success, fail) {
        setTimeout(function() {
            if (true) {
                success('woohoo!');
            } else {
                fail('wahwah');
            }
        }, 1000);
    }

    function exampleFn2() {
        var deferred = Sonoma.Promise.defer();
        doSomethingAsync(deferred.resolve, deferred.reject);
        return deferred.promise;
    }
    ```

    See https://github.com/tildeio/rsvp.js for full feature documentation.

    @module Sonoma
    @submodule Promise

**/

/*global Sonoma, RSVP */
(function (Sonoma, RSVP) {

    RSVP.Promise.prototype.done = function done() {
        Sonoma.Log.warn([
            "Deprecation warning: 'done' will be removed in the next major release. ",
            "Please switch to using 'then' as outlined in the Promises/A+ specification."
        ].join(''));
        return RSVP.Promise.prototype.then.apply(this, arguments);
    };

    RSVP.Promise.prototype.fail = function fail() {
        Sonoma.Log.warn([
            "Deprecation warning: 'fail' will be removed in the next major release. ",
            "Please switch to using 'catch' (or 'caught') as outlined in the Promises/A+ specification."
        ].join(''));
        return RSVP.Promise.prototype.catch.apply(this, arguments);
    };

    Sonoma.Promise = RSVP;

    Sonoma.Promise.resolve = function resolve() {
        var resolveArgs = arguments;
        return new Sonoma.Promise.Promise(function (res, rej) {
            res.apply(this, resolveArgs);
        });
    };

    Sonoma.Promise.reject = function reject() {
        var rejectArgs = arguments;
        return new Sonoma.Promise.Promise(function (res, rej) {
            rej.apply(this, rejectArgs);
        });
    };

}(Sonoma, RSVP));
/// <reference path="sonoma.js" />
/// <reference path="core.js" />
/// <reference path="xml.js" />
/*globals Sonoma*/
/**
    Fetch Module
    @module Sonoma
    @submodule Fetch
    @requires Core, Xml
**/
/**
    FetchXML Utilities for creating FetchXML in JavaScript
    @class Fetch
    @namespace Sonoma
**/
Sonoma.Fetch = (function () {
    "use strict";
    /**
        FetchXML Operators

        | Key | Value |
        | --- |------ |
        | isBetween | between |
        | isEqualTo | eq |
        | isEqualToBusinessUnitId | eq-businessid |
        | isEqualToUserId | eq-userid |
        | isGreaterThan | gt |
        | isGreaterThanOrEqualTo | ge |
        | isIn | in |
        | isLessThan | lt |
        | isLessThanOrEqualTo | le |
        | isLike | like |
        | isNotBetween | not-between |
        | isNotEqualTo | ne |
        | isNotEqualToBusinessUnitId | ne-businessid |
        | isNotEqualToUserId | ne-userid |
        | isNotIn | not-in |
        | isNotLike | not-like |
        | isNotNull | not-null |
        | isNull | null |
        | isOn | on |
        | isOnOrAfter | on-or-after |
        | isOnOrBefore | on-or-before |
        | isWithinLast7Days | last-seven-days |
        | isWithinLastMonth | last-month |
        | isWithinLastXDays | last-x-days |
        | isWithinLastXHours | last-x-hours |
        | isWithinLastXMonths | last-x-months |
        | isWithinLastXWeeks | last-x-weeks |
        | isWithinLastXYears | last-x-years |
        | isWithinLastYear | last-year |
        | isWithinNext7Days | next-seven-days |
        | isWithinNextMonth | next-month |
        | isWithinNextWeek | next-week |
        | isWithinNextXDays | next-x-days |
        | isWithinNextXHours | next-x-hours |
        | isWithinNextXMonths | next-x-months |
        | isWithinNextXWeeks | next-x-weeks |
        | isWithinNextXYears | next-x-years |
        | isWithinNextYear | next-year |
        | isWithinThisMonth | this-month |
        | isWithinThisWeek | this-week |
        | isWithinThisYear | this-year |
        | isWithinToday | today |
        | isWithinTomorrow | tomorrow |
        | isWithinYesterday | yesterday

        @property Operators
        @since 2.0
        @type Object
    **/
    var Operators = {
        isBetween: "between",
        isEqualTo: "eq",
        isEqualToBusinessUnitId: "eq-businessid",
        isEqualToUserId: "eq-userid",
        isGreaterThan: "gt",
        isGreaterThanOrEqualTo: "ge",
        isIn: "in",
        isLessThan: "lt",
        isLessThanOrEqualTo: "le",
        isLike: "like",
        isNotBetween: "not-between",
        isNotEqualTo: "ne",
        isNotEqualToBusinessUnitId: "ne-businessid",
        isNotEqualToUserId: "ne-userid",
        isNotIn: "not-in",
        isNotLike: "not-like",
        isNotNull: "not-null",
        isNull: "null",
        isOn: "on",
        isOnOrAfter: "on-or-after",
        isOnOrBefore: "on-or-before",
        isWithinLast7Days: "last-seven-days",
        isWithinLastMonth: "last-month",
        isWithinLastXDays: "last-x-days",
        isWithinLastXHours: "last-x-hours",
        isWithinLastXMonths: "last-x-months",
        isWithinLastXWeeks: "last-x-weeks",
        isWithinLastXYears: "last-x-years",
        isWithinLastYear: "last-year",
        isWithinNext7Days: "next-seven-days",
        isWithinNextMonth: "next-month",
        isWithinNextWeek: "next-week",
        isWithinNextXDays: "next-x-days",
        isWithinNextXHours: "next-x-hours",
        isWithinNextXMonths: "next-x-months",
        isWithinNextXWeeks: "next-x-weeks",
        isWithinNextXYears: "next-x-years",
        isWithinNextYear: "next-year",
        isWithinThisMonth: "this-month",
        isWithinThisWeek: "this-week",
        isWithinThisYear: "this-year",
        isWithinToday: "today",
        isWithinTomorrow: "tomorrow",
        isWithinYesterday: "yesterday"
    },
    /**
        FetchXML Join Types

        | Key |
        | --- |
        | inner |
        | natural |
        | outer |

        @property JoinTypes
        @since 2.0
        @type Object
    **/
    JoinTypes = {
        inner: "inner",
        natural: "natural",
        outer: "outer"
    };
    /**
        Create an *Attribute*
        @method _newAttribute
        @since 2.0
        @private
        @param name {String}
        @param [alias] {String}
        @param [isDistinct] {Boolean}
    **/
    function _newAttribute(name, isAggregate, alias, isDistinct) {
        if (typeof name === "undefined") {
            throw new Error("name is required.");
        }
        var attribute = new Sonoma.Xml.XmlObject("attribute");
        attribute.addAttribute("name", name);
        if (isAggregate) {
            attribute.addAttribute("aggregate", "countcolumn");
        }
        if (alias) {
            attribute.addAttribute("alias", alias);
        }
        if (isDistinct) {
            attribute.addAttribute("distinct", isDistinct);
        }
        return attribute;
    }
    /**
        Create an *Order*
        @method _newOrder
        @private
        @since 2.0
        @param name {String}
        @param [isDescending] {Boolean} Sort by Descending
    **/
    function _newOrder(attribute, descending) {
        if (typeof attribute === "undefined") {
            throw new Error("attribute is required.");
        }
        var order = new Sonoma.Xml.XmlObject("order");
        order.addAttribute("attribute", attribute);
        order.addAttribute("descending", descending || false);
        return order;
    }
    //#region FetchXml
    /**
        Programatically create FetchXml
        @class FetchXml
        @since 2.0
        @namespace Sonoma.Fetch
        @constructor
        @param entityName {string} LogicalName of the entity.
        @param [count] {number} Number of records to retrieve.
        @param [isAggregate=false] {Boolean} Aggregate results.
        @param [isDistinct=false] {Boolean} Results are distinct.
        @chainable
        @example
            var fetch = Sonoma.Fetch.FetchXml('contact')
                            .addAttribute('ownerid')
                            .toString();
            // → '<fetch mapping='logical' version='1.0'><entity name='contact'><attribute name='ownerid' /></entity></fetch>'
    **/
    function FetchXml(entityName, count, isAggregate, isDistinct) {
        var rootXml, entityXml;
        if (!(this instanceof FetchXml)) {
            return new FetchXml(entityName, count, isAggregate, isDistinct);
        }
        if (typeof entityName === "undefined") {
            throw new Error("entityName is required.");
        }
        rootXml = new Sonoma.Xml.XmlObject("fetch");
        rootXml.addAttribute("mapping", "logical");
        if (count) {
            rootXml.addAttribute("count", count);
        }
        this.isAggregate = typeof isAggregate !== 'undefined' ? isAggregate : false;
        if (this.isAggregate) {
            rootXml.addAttribute("aggregate", this.isAggregate);
        }
        this.isDistinct = typeof isDistinct !== 'undefined' ? isDistinct : false;
        if (this.isDistinct) {
            rootXml.addAttribute("distinct", this.isDistinct);
        }
        rootXml.addAttribute("version", "1.0");
        entityXml = new Sonoma.Xml.XmlObject("entity");
        entityXml.addAttribute("name", entityName);
        rootXml.addElement(entityXml);
        this.rootXml = rootXml;
        this.entityXml = entityXml;
        this.filterAdded = false;
    }
    FetchXml.prototype = (function () {
        /*jshint validthis:true */
        /**
            Adds an *Attribute*
            @method addAttribute
            @since 2.0
            @param name {String}
            @param [alias] {String}
            @param [isDistinct] {Boolean}
            @chainable
        **/
        function addAttribute(name, alias, isDistinct) {
            var attribute = _newAttribute(name, this.isAggregate, alias, isDistinct);
            this.entityXml.addElement(attribute);
            return this;
        }
        /**
            Add an *Order*
            @method addOrder
            @since 2.0
            @param name {String}
            @param [isDescending] {Boolean} Sort by Descending
            @chainable
        **/
        function addOrder(attribute, isDescending) {
            var order = _newOrder(attribute, isDescending);
            this.entityXml.addElement(order);
            return this;
        }
        /**
            Add a *Filter*
            @method addFilter
            @since 2.0
            @param filter {Sonoma.Fetch.Filter}
            @chainable
        **/
        function addFilter(filter) {
            if (!(filter instanceof Filter)) {
                throw new Error("filter must be a Filter type.");
            }
            if (this.filterAdded) {
                throw new Error("Only one filter may be added per fetch. Filters can be nested inside each other.");
            }
            this.entityXml.addElement(filter.rootXml);
            this.filterAdded = true;
            return this;
        }
        /**
            Add a *Link Entity*
            @method addLinkEntity
            @since 2.0
            @param linkEntity {Sonoma.Fetch.LinkEntity}
            @chainable
        **/
        function addLinkEntity(linkEntity) {
            if (!(linkEntity instanceof LinkEntity)) {
                throw new Error("linkEntity must be a LinkEntity type.");
            }
            this.entityXml.addElement(linkEntity.rootXml);
            return this;
        }
        /**
            Create a *string* of *FetchXML*
            @method toString
            @since 2.0
            @param [isPretty=false] {Boolean}
            @return {String} FetchXML
        **/
        function toString(isPretty) {
            return this.rootXml.toString(isPretty);
        }
        //FetchXml methods
        return {
            addAttribute: addAttribute,
            addOrder: addOrder,
            addFilter: addFilter,
            addLinkEntity: addLinkEntity,
            toString: toString
        };
    })();
    //#endregion
    //#region Filter
    /**
        Programatically create FetchXml Filter
        @class Filter
        @since 2.0
        @namespace Sonoma.Fetch
        @constructor
        @param [isOr] {Boolean} Filter on OR instead of AND
        @chainable
        @example
            var filter = Sonoma.Fetch.Filter()
                            .addCondition('fullname', Sonoma.Fetch.Operator.isEqualTo, 'Sonoma');
            var fetch = Sonoma.Fetch.FetchXml('contact')
                            .addFilter(filter)
                            .toString();
            // → '<fetch mapping='logical' version='1.0'><entity name='contact'><filter><condition attribute="fullname" operator="eq" value="Sonoma" /></filter></fetch>'
    **/
    function Filter(isOr) {
        if (!(this instanceof Filter)) {
            return new Filter(isOr);
        }
        var rootXml = new Sonoma.Xml.XmlObject("filter");
        if (isOr) {
            rootXml.addAttribute("type", "or");
        }
        this.rootXml = rootXml;
    }
    Filter.prototype = (function () {
        /*jshint validthis:true */
        /**
            Add a *Condition*
            @method addCondition
            @since 2.0
            @param attribute {string}
            @param operator {string} use Sonoma.Fetch.Operator
            @param [values] {string, null, Array}
            @chainable
        **/
        function addCondition(attribute, operator, values) {
            var condition,
                i = 0, valueXml;
            if (typeof attribute === "undefined") {
                throw new Error("attribute is required.");
            }
            if (typeof operator === "undefined") {
                throw new Error("operator is required.");
            }
            condition = new Sonoma.Xml.XmlObject("condition");
            condition.addAttribute("attribute", attribute);
            condition.addAttribute("operator", operator);
            if (values instanceof Array) {
                for (i; i < values.length; ++i) {
                    valueXml = new Sonoma.Xml.XmlObject("value");
                    valueXml.addElement(values[i]);
                    condition.addElement(valueXml);
                }
            } else if (values) {
                condition.addAttribute("value", values);
            }
            this.rootXml.addElement(condition);
            return this;
        }
        /**
            Add a *Filter*
            @method addFilter
            @since 2.0
            @param filter {Sonoma.Fetch.Filter}
            @returns updatedFilter {Sonoma.Fetch.Filter}
            @chainable
        **/
        function addFilter(filter) {
            if (!(filter instanceof Filter)) {
                throw new Error("filter must be a Filter type.");
            }
            this.rootXml.addElement(filter.rootXml);
            return this;
        }
        //Filter methods
        return {
            addCondition: addCondition,
            addFilter: addFilter
        };
    })();
    //#endregion
    //#region LinkEntity
    /**
        Programatically create FetchXml LinkEntity
        @class LinkEntity
        @since 2.0
        @namespace Sonoma.Fetch
        @constructor
        @param toEntity {string}
        @param fromAttribute {string}
        @param toAttribute {string}
        @param [joinType] {string} Use enum at Sonoma.Fetch.JoinTypes
        @param [alias] {string}
        @chainable
        @example
            var fetch = Sonoma.Fetch.FetchXml('contact');
            var linkEntity = Sonoma.Fetch.LinkEntity('account', 'contact_account', 'account_contact');
            fetch.addLinkEntity(linkEntity)
                .toString();
            // → '<fetch mapping='logical' version='1.0'><entity name='contact'><link-entity name='account' from='contact_account' to='account_contact' /></entity></fetch>'
    **/
    function LinkEntity(toEntity, fromAttribute, toAttribute, joinType, alias) {
        if (!(this instanceof LinkEntity)) {
            return new LinkEntity(toEntity, fromAttribute, toAttribute, alias, joinType);
        }
        if (typeof toEntity === "undefined") {
            throw new Error("toEntity is required.");
        }
        if (typeof fromAttribute === "undefined") {
            throw new Error("fromAttribute is required.");
        }
        if (typeof toAttribute === "undefined") {
            throw new Error("toAttribute is required.");
        }
        var rootXml = new Sonoma.Xml.XmlObject("link-entity");
        rootXml.addAttribute("name", toEntity);
        rootXml.addAttribute("from", fromAttribute);
        rootXml.addAttribute("to", toAttribute);
        if (alias) {
            rootXml.addAttribute("alias", alias);
        }
        if (joinType) {
            rootXml.addAttribute("link-type", joinType);
        }
        this.rootXml = rootXml;
        this.filterAdded = false;
    }
    LinkEntity.prototype = (function () {
        /*jshint validthis:true */
        /**
            Adds an *Attribute*
            @method addAttribute
            @since 2.0
            @param name {String}
            @chainable
        **/
        function addAttribute(name) {
            var attribute = _newAttribute(name);
            this.rootXml.addElement(attribute);
            return this;
        }
        /**
            Add an *Order*
            @method addOrder
            @since 2.0
            @param attributeName {String}
            @param [isDescending] {Boolean} Sort by Descending
            @chainable
        **/
        function addOrder(attribute, isDescending) {
            var order = _newOrder(attribute, isDescending);
            this.rootXml.addElement(order);
            return this;
        }
        /**
            Add a *Filter*
            @method addFilter
            @since 2.0
            @param filter {Sonoma.Fetch.Filter}
            @chainable
        **/
        function addFilter(filter) {
            if (!(filter instanceof Filter)) {
                throw new Error("filter must be a Filter type.");
            }
            if (this.filterAdded) {
                throw new Error("Only one filter may be added per fetch. Filters can be nested inside each other.");
            }
            this.rootXml.addElement(filter.rootXml);
            this.filterAdded = true;
            return this;
        }
        /**
            Add a *Link Entity*
            @method addLinkEntity
            @since 2.0
            @param linkEntity {Sonoma.Fetch.LinkEntity}
            @chainable
        **/
        function addLinkEntity(linkEntity) {
            if (!(linkEntity instanceof LinkEntity)) {
                throw new Error("linkEntity must be a LinkEntity type.");
            }
            this.rootXml.addElement(linkEntity.rootXml);
            return this;
        }
        //LinkEntity methods
        return {
            addAttribute: addAttribute,
            addOrder: addOrder,
            addFilter: addFilter,
            addLinkEntity: addLinkEntity
        };
    })();
    //#endregion
    return {
        //constructors
        FetchXml: FetchXml,
        Filter: Filter,
        LinkEntity: LinkEntity,
        //enums
        Operators: Operators,
        JoinTypes: JoinTypes
    };
}());

/// <reference path="sonoma.js" />
/// <reference path="core.js" />
/// <reference path="xml.js" />
/// <reference path="fetch.js" />
/*global Sonoma*/
Sonoma.QueryBuilder = (function () {
    "use strict";
    function Query() {
        //TODO: Intellisense Comments
        //TODO: Input Checking
        this.entityName = null;
        this.attributes = [];
        this.orders = [];
        this.filters = [];
        this.joins = [];
    }
    Query.prototype = (function () {
        function From(entityName) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            if (typeof entityName !== "string") {
                throw new Error("entityName must be a string.");
            }
            this.entityName = entityName;
            return this;
        }
        function Select(attributes) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            if (attributes instanceof Array) {
                for (var i = 0; i < attributes.length; ++i) {
                    this.attributes.push(attributes[i]);
                }
            } else if (typeof attributes === "string") {
                this.attributes.push(attributes);
            }
            return this;
        }
        function SelectAll() {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            this.attributes = [];
            return this;
        }
        function Join(toEntity, fromAttribute, toAttribute, joinType, conditions, alias) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            var join = {
                toEntity: toEntity,
                fromAttribute: fromAttribute,
                toAttribute: toAttribute,
                joinType: joinType,
                alias: alias
            },
            filters = [], i = 0;
            for (; i < conditions.length; ++i) {
                filters.push(conditions[i]);
            }
            join.filters = { conditions: filters };
            //join.joins = joins || [];
            this.joins.push(join);
            return this;
        }
        function Where(attribute, condition, value) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            this.filters.push({ conditions: [new Condition(attribute, condition, value)] });
            return this;
        }
        function WhereAllTrue(conditions) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            var whereAll = [], i = 0;
            for (; i < conditions.length; ++i) {
                whereAll.push(conditions[i]);
            }
            this.filters.push({ conditions: whereAll });
            return this;
        }
        function WhereAnyTrue(conditions) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            var whereAny = [], i = 0, condition;
            for (0; i < conditions.length; ++i) {
                condition = new Condition(conditions[i]);
                whereAny.push(condition);
            }
            this.filters.push({ isOr: true, conditions: whereAny });
            return this;
        }
        function WhereMultiple() {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            //TODO: Implement Method
            return this;
        }
        function Order(attribute, isDescending) {
            //TODO: Intellisense Comments
            //TODO: Input Checking
            var order = {};
            order.attribute = attribute;
            order.isDescending = isDescending;
            this.orders.push(order);
            return this;
        }
        function _addJoinToFetch(fetch, join) {
            var linkEntity = new Sonoma.Fetch.LinkEntity(join.toEntity, join.fromAttribute, join.toAttribute, join.joinType, join.alias),
                filter = _filterToFetchFilter(join.filters);
            linkEntity.addFilter(filter);
            fetch.addLinkEntity(linkEntity);
            //for (var i = 0; i < join.joins.length; ++i) {
            //    _addJoinToFetch(linkEntity, join.joins[i]);
            //}
            return;
        }
        function _filterToFetchFilter(filter) {
            var fetchFilter = new Sonoma.Fetch.Filter(filter.isOr),
                i = 0, condition;
            for (; i < filter.conditions.length; ++i) {
                condition = filter.conditions[i];
                fetchFilter.addCondition(condition.attribute, condition.operator, condition.values);
            }
            return fetchFilter;
        }
        function toString() {
            /*jshint validthis:true */
            //TODO: Intellisense Comments
            //TODO: Input Checking
            var fetch = new Sonoma.Fetch.FetchXml(this.entityName),
                i = 0, filter;

            for (i = 0; i < this.attributes.length; ++i) {
                fetch.addAttribute(
                    this.attributes[i]);
            }
            for (i = 0; i < this.orders.length; ++i) {
                fetch.addOrder(
                    this.orders[i].attribute, this.orders[i].isDescending);
            }
            for (i = 0; i < this.filters.length; ++i) {
                filter = _filterToFetchFilter(this.filters[i]);
                fetch.addFilter(filter);
            }
            //TODO: Implement Tostring Join
            for (i = 0; i < this.joins.length; ++i) {
                _addJoinToFetch(fetch, this.joins[i]);
            }
            return fetch.toString();
        }
        return {
            From: From,
            Select: Select,
            SelectAll: SelectAll,
            Join: Join,
            Order: Order,
            toString: toString,
            Where: Where
        };
    })();
    function Condition(attribute, operator, values) {
        //TODO: Intellisense Comments
        //TODO: Input Checking
        this.attribute = attribute;
        this.operator = operator;
        this.values = values;
    }
    Sonoma.QB = Sonoma.QueryBuilder;
    return {
        Query: Query,
        Condition: Condition
    };
}());
/// <reference path='sonoma.js' />
/*global Sonoma XMLHttpRequest Q */
/**
    OrgService Module
    @module Sonoma
    @submodule OrgService
**/
/**
    OrgService Utilities
    @class OrgService
    @namespace Sonoma
**/
Sonoma.OrgService = (function () {
    'use strict';

    //#region SOAP Methods

    var _namespaces = {
            xrm: 'xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts"',
            crm: 'xmlns:c="http://schemas.microsoft.com/crm/2011/Contracts"',
            collection: 'xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic"',
            arrays: 'xmlns:b="http://schemas.microsoft.com/2003/10/Serialization/Arrays"',
            xml: 'xmlns:c="http://www.w3.org/2001/XMLSchema"',
            serialization: 'xmlns:c="http://schemas.microsoft.com/2003/10/Serialization/"'
        },
        attributeTypes = {
            Money: 'Money',
            OptionSetValue: 'OptionSetValue',
            Boolean: 'Boolean',
            EntityReference: 'EntityReference',
            DateTime: 'DateTime',
            Decimal: 'Decimal',
            Double: 'Double',
            Guid: 'Guid'
        };

    function _buildColumnSet(columns) {
        var retrieveAllColumns, columnSet, i, len;

        retrieveAllColumns = (Sonoma.type(columns) !== 'array');
        columnSet = [
            '<columnSet ', _namespaces.xrm, '>',
                '<a:AllColumns>', retrieveAllColumns.toString(), '</a:AllColumns>',
                '<a:Columns ', _namespaces.arrays, '>'
        ];

        if (!retrieveAllColumns) {
            len = columns.length;
            for (i = 0; i < len; i++) {
                if (Sonoma.type(columns[i]) === 'string') {
                    columnSet.push(Sonoma.String.format('<b:string>{0}</b:string>', columns[i]));
                }
            }
        }

        columnSet.push('</a:Columns>');
        columnSet.push('</columnSet>');
        return columnSet.join('');
    }

    function _mergeObjects(o1, o2) {
        var o3 = {},
            prop;

        for (prop in o1) {
            if (o1.hasOwnProperty(prop)) {
                o3[prop] = o1[prop];
            }
        }
        for (prop in o2) {
            if (o2.hasOwnProperty(prop)) {
                o3[prop] = o2[prop];
            }
        }

        return o3;
    }

    function _isUndefined(o) {
        return (typeof (o) === 'undefined');
    }

    function _isNull(o) {
        return (o === null);
    }

    function _buildAttributeXml(entity) {
        var attributeXml, attr, attrValue;

        attributeXml = [];
        for (attr in entity) {
            if (attr === 'Metadata' || !entity.hasOwnProperty(attr)) {
                return;
            }

            attributeXml.push('<a:KeyValuePairOfstringanyType>');
            attributeXml.push(Sonoma.String.format('<b:key>{0}</b:key>', attr));

            attrValue = entity[attr];

            if (Sonoma.type(attrValue) === 'undefined' || attrValue === null) {
                alert('To set an attribute to null, set its value to a new instance of the OrgService.NullValue class.');
                return;
            }

            switch (Sonoma.type(attrValue)) {
                case 'string':
                    attributeXml.push('<b:value i:type="c:string" ' + _namespaces.xml + '>' +
                        Sonoma.Xml.xmlEncode(entity[attr]) +
                        '</b:value>');
                    break;
                case 'number':
                    attributeXml.push('<b:value i:type="c:int" ' + _namespaces.xml + '>' +
                        entity[attr] +
                        '</b:value>');
                    break;
                default:
                    if (attrValue instanceof CRMAttribute === false) {
                        alert('The attribute ' + attr + ' is a complex type, but can not be serialized.  Make sure you define the attribute using the appropriate class (new Sonoma.OrgService.[Boolean|DateTime|Decimal|EntityReference|Guid|OptionSetValue]).');
                    }

                    attributeXml.push(attrValue.toXml());
            }

            attributeXml.push('</a:KeyValuePairOfstringanyType>');
        }

        return attributeXml.join('');
    }
    
    function _buildEntityContentsXml(logicalName, id, entity) {
        if (Sonoma.type(entity) !== 'object') {
            alert('Entity is not an object, cannot continue operation.');
            return;
        }
        
        if (_isNull(logicalName)) {
            alert('LogicalName was not specified, cannot continue operation.');
            return;
        }

        if (_isNull(id)) {
            id = '00000000-0000-0000-0000-000000000000';
        }

        var request = [
                '<a:Attributes ', _namespaces.collection, '>',
                    _buildAttributeXml(entity),
                '</a:Attributes>',
                '<a:EntityState i:nil="true" />',
                '<a:FormattedValues ', _namespaces.collection, ' />',
                '<a:Id>' + id + '</a:Id>',
                '<a:LogicalName>' + logicalName + '</a:LogicalName>',
                '<a:RelatedEntities ', _namespaces.collection, ' />',
            ];

        return request.join('');
    }

    function _buildEntityXml(logicalName, id, entity) {
        var request = [
                '<entity ', _namespaces.xrm, '>',
                    _buildEntityContentsXml(logicalName, id, entity),
                '</entity>'
            ];

        return request.join('');
    }

    function _getChildNode(contextNode, childName) {
        if (!contextNode || !contextNode.hasChildNodes()) {
            return null;
        }
        return contextNode.querySelector(contextNode.localName + ' > ' + childName);
    }

    function _getChildNodeText(contextNode, childName) {
        var node = _getChildNode(contextNode, childName);
        return (node) ? node.textContent : '';
    }

    function _parseOptionSetValue(formattedValues, attributeData, attributeKey, isSubAttribute) {
        var value;

        value = new Sonoma.OrgService.OptionSetValue();

        if (!isSubAttribute) {
            value.Value = parseInt(_getChildNodeText(attributeData, 'value > Value'), 10);
        }
        else {
            value.Value = parseInt(_getChildNodeText(attributeData, 'value > Value > Value'), 10);
        }

        if (formattedValues && formattedValues.hasOwnProperty(attributeKey)) {
            value.Label = formattedValues[attributeKey];
        }

        return value;
    }

    function _parseEntityReference(attributeData, isSubAttribute) {
        var value, path;

        value = new Sonoma.OrgService.EntityReference();
        path = 'value > ';

        if (isSubAttribute) {
            path = 'value > Value > ';
        }

        if (!_isNull(_getChildNode(attributeData, path + 'Id'))) {
            value.Id = _getChildNodeText(attributeData, path + 'Id');
        }

        if (!_isNull(_getChildNode(attributeData, path + 'Name'))) {
            value.Name = _getChildNodeText(attributeData, path + 'Name');
        }

        if (!_isNull(_getChildNode(attributeData, path + 'LogicalName'))) {
            value.LogicalName = _getChildNodeText(attributeData, path + 'LogicalName');
        }

        return value;
    }

    function _parseMoney(formattedValues, attributeData, attributeKey, isSubAttribute) {
        var value, path;

        value = new Sonoma.OrgService.Money();
        path = 'value > ';

        if (isSubAttribute) {
            path = 'value > Value > ';
        }

        value.Value = parseFloat(_getChildNodeText(attributeData, path + 'Value'));

        if (formattedValues && formattedValues.hasOwnProperty(attributeKey)) {
            value.DisplayValue = formattedValues[attributeKey];
        }

        return value;
    }

    function _parseGuid(attributeData, isSubAttribute) {
        var value, path;

        value = new Sonoma.OrgService.Guid();
        path = 'value';

        if (isSubAttribute) {
            path = 'value > Value';
        }

        value.Value = _getChildNodeText(attributeData, path);
        return value;
    }

    function _parseInt(attributeData, isSubAttribute) {
        var result;

        if (!isSubAttribute) {
            result = parseInt(_getChildNodeText(attributeData, 'value'), 10);
        }
        else {
            result = parseInt(_getChildNodeText(attributeData, 'value > Value'), 10);
        }

        return result;
    }

    function _parseDecimal(formattedValues, attributeData, attributeKey, isSubAttribute) {
        var value, path;

        value = new Sonoma.OrgService.Decimal();
        path = 'value';

        if (isSubAttribute) {
            path = 'value > Value';
        }

        value.Value = parseFloat(_getChildNodeText(attributeData, path), 10);
        if (formattedValues && formattedValues.hasOwnProperty(attributeKey)) {
            value.DisplayValue = formattedValues[attributeKey];
        }

        return value;
    }

    function _parseDouble(formattedValues, attributeData, attributeKey, isSubAttribute) {
        var value, path;

        value = new Sonoma.OrgService.Double();
        path = 'value';

        if (isSubAttribute) {
            path = 'value > Value';
        }

        value.Value = parseFloat(_getChildNodeText(attributeData, path), 10);
        if (formattedValues && formattedValues.hasOwnProperty(attributeKey)) {
            value.DisplayValue = formattedValues[attributeKey];
        }

        return value;
    }

    function _parseBoolean(formattedValues, attributeData, attributeKey, isSubAttribute) {
        var value, path;

        value = new Sonoma.OrgService.Boolean();
        path = 'value';

        if (isSubAttribute) {
            path = 'value > Value';
        }

        value.Value = (_getChildNodeText(attributeData, path) === 'true');
        if (formattedValues && formattedValues.hasOwnProperty(attributeKey)) {
            value.DisplayValue = formattedValues[attributeKey];
        }

        return value;
    }

    function _parseDate(formattedValues, attributeData, attributeKey, isSubAttribute) {
        var value, path, tempVal;

        value = new Sonoma.OrgService.DateTime();
        path = 'value';

        if (isSubAttribute) {
            path = 'value > Value';
        }

        value.UTC = _getChildNodeText(attributeData, path);
        if (formattedValues && formattedValues.hasOwnProperty(attributeKey)) {
            value.DisplayValue = formattedValues[attributeKey];

            tempVal = Sonoma.Date.parse(value.UTC);
            value.Value = new Date(tempVal);
        }

        return value;
    }

    function _parseAliasedKey(attributeData) {
        var alias, aliasedParts;

        alias = _getChildNodeText(attributeData, 'key');
        aliasedParts = alias.split('.');

        if (aliasedParts.length !== 2) {
            return null;
        }
        else {
            return aliasedParts[0];
        }
    }

    function _parseAliasedValue(formattedValues, attributeData, attributeKey) {
        var value, attributeName, attributeType, attributeValue;

        value = { 
            _type: 'Entity',
            EntityLogicalName: _getChildNodeText(attributeData, 'value > EntityLogicalName')
        };
        attributeName = _getChildNodeText(attributeData, 'value > AttributeLogicalName');
        attributeType = _getChildNode(attributeData, 'value > Value').getAttribute('i:type');
        attributeValue = _getChildNodeText(attributeData, 'value > Value');

        switch (attributeType) {
            case 'c:guid':
                attributeValue = _parseGuid(attributeData, true);
                break;
            case 'a:OptionSetValue':
                attributeValue = _parseOptionSetValue(formattedValues, attributeData, attributeKey, true);
                break;
            case 'a:EntityReference':
                attributeValue = _parseEntityReference(attributeData, true);
                break;
            case 'a:Money':
                attributeValue = _parseMoney(formattedValues, attributeData, attributeKey, true);
                break;
            case 'c:dateTime':
                attributeValue = _parseDate(formattedValues, attributeData, attributeKey, true);
                break;
            case 'c:decimal':
                attributeValue = _parseDecimal(formattedValues, attributeData, attributeKey, true);
                break;
            case 'c:double':
                attributeValue = _parseDouble(formattedValues, attributeData, attributeKey, true);
                break;
            case 'c:int':
                attributeValue = _parseInt(attributeData, true);
                break;
            case 'c:boolean':
                attributeValue = _parseBoolean(formattedValues, attributeData, attributeKey, true);
                break;
            case 'a:AliasedValue':
                alert('Unsupported parsing of multi-tiered aliased/linked entities');
                break;
            case 'c:string':
                break;
            default:
                break;
        }
        if (attributeName) {
            value[attributeName] = attributeValue;
        }

        return value;
    }

    function _parsePaging(xml) {
        var paging, pagingXml;

        paging = { MoreRecords: false, TotalRecordCount: -1, TotalRecordCountLimitExceeded: false, PagingCookie: null };
        pagingXml = xml.querySelector('Envelope > Body > RetrieveMultipleResponse > RetrieveMultipleResult');

        if (_getChildNode(pagingXml, 'TotalRecordCount') && !isNaN(parseInt(_getChildNodeText(pagingXml, 'TotalRecordCount'), 10))) {
            paging.TotalRecordCount = parseInt(_getChildNodeText(pagingXml, 'TotalRecordCount'), 10);
        }

        if (_getChildNode(pagingXml, 'TotalRecordCountLimitExceeded')) {
            paging.TotalRecordCountLimitExceeded = (_getChildNodeText(pagingXml, 'TotalRecordCountLimitExceeded') === 'true');
        }

        if (_getChildNode(pagingXml, 'PagingCookie')) {
            paging.PagingCookie = _getChildNodeText(pagingXml, 'PagingCookie').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
        }

        if (_getChildNode(pagingXml, 'MoreRecords')) {
            paging.MoreRecords = (_getChildNodeText(pagingXml, 'MoreRecords') === 'true');
        }

        return paging;
    }
    
    function _parseAttributeCollection(entity, attributesNode, formattedValues) {
        var i = 0,
            len = attributesNode.childNodes.length,
            attributeData,
            attributeType,
            attributeValue,
            attributeKey;
        
        for (; i < len; i++) {
            attributeData = attributesNode.childNodes[i];
            attributeType = _getChildNode(attributeData, 'value').getAttribute('i:type');
            attributeValue = _getChildNodeText(attributeData, 'value');
            attributeKey = _getChildNodeText(attributeData, 'key');
            switch (attributeType) {
                case 'c:guid':
                    attributeValue = _parseGuid(attributeData);
                    break;
                case 'a:OptionSetValue':
                    attributeValue = _parseOptionSetValue(formattedValues,
                        attributeData, attributeKey);
                    break;
                case 'a:EntityReference':
                    attributeValue = _parseEntityReference(attributeData);
                    break;
                case 'a:Money':
                    attributeValue = _parseMoney(formattedValues,
                        attributeData, attributeKey);
                    break;
                case 'c:dateTime':
                    attributeValue = _parseDate(formattedValues,
                        attributeData, attributeKey);
                    break;
                case 'c:decimal':
                    attributeValue = _parseDecimal(formattedValues,
                        attributeData, attributeKey);
                    break;
                case 'c:double':
                    attributeValue = _parseDouble(formattedValues,
                        attributeData, attributeKey);
                    break;
                case 'c:int':
                    attributeValue = _parseInt(attributeData);
                    break;
                case 'c:boolean':
                    attributeValue = _parseBoolean(formattedValues,
                        attributeData, attributeKey);
                    break;
                case 'a:AliasedValue':
                    entity.Metadata.RelatedEntityCount++;
                    attributeValue = _parseAliasedValue(formattedValues, attributeData, attributeKey);
                    attributeKey = _parseAliasedKey(attributeData);
                    // Correct linked aliased attributes
                    if (!attributeKey) {
                        entity[_getChildNodeText(attributeData, 'key')] =
                            attributeValue[_getChildNodeText(attributeData, 'value > AttributeLogicalName')];
                    }

                    if (entity.hasOwnProperty(attributeKey)) {
                        entity[attributeKey] = _mergeObjects(entity[attributeKey], attributeValue);
                    }
                    else if (attributeKey) {
                        entity[attributeKey] = attributeValue;
                    }

                    attributeKey = null;
                    break;
                case 'c:string':
                    break;
                default:
                    break;
            }

            if (attributeKey) {
                entity[attributeKey] = attributeValue;
            }
        }
    }

    function _parseEntity(entityXml, fields) {
        var entity, formattedValues, formattedValuesXml, attributesData,
            attributeValue, attributeKey,
            i, len;

        entity = {
            Metadata: {
                RelatedEntityCount: 0,
                AttributeCount: 0
            }
        };

        if (Sonoma.type(fields) === 'array') {
            len = fields.length;
            for (i = 0; i < len; i++) {
                entity[fields[i]] = null;
            }
        }

        formattedValues = {};
        formattedValuesXml = _getChildNode(entityXml, 'FormattedValues');

        if (formattedValuesXml != null && formattedValuesXml.childNodes.length > 0) {
            len = formattedValuesXml.childNodes.length;
            for (i = 0; i < len; i++) {
                attributeKey = _getChildNodeText(formattedValuesXml.childNodes[i], 'key');
                attributeValue = _getChildNodeText(formattedValuesXml.childNodes[i], 'value');

                if (attributeKey) {
                    formattedValues[attributeKey] = attributeValue;
                }
            }
        }

        attributesData = _getChildNode(entityXml, 'Attributes');
        if (attributesData.childNodes.length > 0) {
            //There are attributes
            entity.Metadata.AttributeCount = attributesData.childNodes.length;
            entity.Metadata.RelatedEntityCount = 0;

            _parseAttributeCollection(entity, attributesData, formattedValues);
        }

        entity.Metadata.Id = _getChildNodeText(entityXml, 'Id');
        entity.Metadata.LogicalName = _getChildNodeText(entityXml, 'LogicalName');
        // Current version doesn't utilize RelatedEntities collection
        //entity.RelatedEntities = null;
        return entity;
    }

    function _parseCreate(xml) {
        var id = xml.querySelector('Envelope > Body > CreateResponse > CreateResult').textContent;

        return id;
    }

    function _parseRetrieve(xml, fields) {
        var entity;

        entity = _parseEntity(xml.querySelector('Envelope > Body > RetrieveResponse > RetrieveResult'), fields);

        return entity;
    }

    function _parseRetrieveMultiple(xml) {
        var paging, entitiesXml, entities, i, len;

        paging = _parsePaging(xml);
        entitiesXml = xml.querySelector('Envelope > Body > RetrieveMultipleResponse > RetrieveMultipleResult > Entities');
        entities = [];

        if (entitiesXml.childNodes.length > 0) {
            len = entitiesXml.childNodes.length;
            for (i = 0; i < len; i++) {
                entities.push(_parseEntity(entitiesXml.childNodes[i]));
            }

            return { Entities: entities, Paging: paging };
        }
        else {
            return { Entities: [], Paging: null };
        }
    }

    function _parseExecuteWorkflowRequest(xml) {
        var node = xml.querySelector('Envelope > Body > ExecuteResponse > ExecuteResult > Results > KeyValuePairOfstringanyType > value'),
            id = (node) ? node.textContent : null;

        return id;
    }

    function _parseInitializeFromRequest(xml) {
        var entity = _parseEntity(xml.querySelector('Envelope > Body > ExecuteResponse > ExecuteResult > Results > KeyValuePairOfstringanyType > value'));

        return entity;
    }

    // Private function that creates a return object for synchronous calls
    function _getReturnObject(successFlag, returnValue) {
        return {
            Success: successFlag,
            Value: returnValue
        };
    }

    function _buildGenericRequest(type, name, parameters) {
        var request = [
                '<request i:type="c:' + type + '" ',
                        _namespaces.xrm, ' ', _namespaces.crm, '>',
                    '<a:Parameters ', _namespaces.collection, '>',
                        parameters,
                    '</a:Parameters>',
                    '<a:RequestId i:nil="true" />',
                    '<a:RequestName>', name, '</a:RequestName>',
                '</request>'
            ];

        return request.join('');
    }

    function _buildKeyValueXmlPair(name, value) {
        var pair = [
                '<a:KeyValuePairOfstringanyType>',
                '<b:key>', name, '</b:key>',
                value,
                '</a:KeyValuePairOfstringanyType>'
            ];

        return pair.join('');
    }
    
    function _handleResponse(req, parser, successCallback, errorCallback) {
        var response;

        if (req.readyState === 4) {
            if (req.status === 200) {
                response = Sonoma.Xml.loadXml(req.responseText);

                if (parser) {
                    response = parser(response);
                }

                successCallback(response);
            }
            else if (req.status === 0) {
                // Safari cancels Ajax requests when you navigate away from a
                //  page with a status of zero.
                req = null;
                return;
            }
            else {
                errorCallback(_getError(req));
            }
        }

        req = null;
    }

    function _getError(response) {
        var errorMessage, operationStatus,
            bodyNode, node, faultStringNode, operationStatusValue,
            i, iLength, j, jLength, k, kLength, error,
            faultXml = Sonoma.Xml.loadXml(response.responseText);

        errorMessage = 'Unknown Error (Unable to parse the fault)';
        operationStatus = 0;

        if (Sonoma.type(faultXml) !== 'object') {
            return;
        }
        
        try {
            bodyNode = faultXml.firstChild.firstChild;
            //Retrieve the fault node
            iLength = bodyNode.childNodes.length;
            for (i = 0; i < iLength; i++) {
                node = bodyNode.childNodes[i];

                //NOTE: This comparison does not handle the case where the XML namespace changes
                if ('s:Fault' === node.nodeName) {
                    jLength = node.childNodes.length;
                    for (j = 0; j < jLength; j++) {
                        faultStringNode = node.childNodes[j];
                        if ('faultstring' === faultStringNode.nodeName) {
                            errorMessage = faultStringNode.textContent;
                        }

                        //Find the exception OperationStatus
                        kLength = faultStringNode.length;
                        for (k = 0; k < kLength; k++) {
                            if ('ErrorDetails' === faultStringNode.nodeName) {
                                if (operationStatus === '2') {
                                    return;
                                }

                                // TODO: not sure exactly what 'this' is in the current context,
                                // should refactor when possible
                                /*ignore jslint start*/
                                /*jshint ignore:start*/
                                operationStatusValue = this.childNodes[1].textContent;
                                if ('OperationStatus' === this.childNodes[0].textContent) {
                                    if (!operationStatus || operationStatus < operationStatusValue) {
                                        operationStatus = operationStatusValue;
                                    }
                                }
                                /*jshint ignore:end*/
                                /*ignore jslint end*/
                            }
                        }
                    }
                    break;
                }
            }
        }
        catch (e) { }

        error = new Error(errorMessage);
        error.operationStatus = operationStatus;
        return error;
    }

    //#endregion
    //#region Service Methods

    //#region Extension Methods

    function _assignInternal(assigneeEntityReference, targetEntityReference, isAsync) {
        var request = [
                '<request i:type="c:AssignRequest" xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts" xmlns:c="http://schemas.microsoft.com/crm/2011/Contracts">',
                    '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>Target</b:key>',
                            targetEntityReference.toXml(),
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>Assignee</b:key>',
                            assigneeEntityReference.toXml(),
                        '</a:KeyValuePairOfstringanyType>',
                    '</a:Parameters>',
                    '<a:RequestId i:nil="true" />',
                    '<a:RequestName>Assign</a:RequestName>',
                '</request>'
        ].join('');

        if (isAsync) {
            return execute(request, 'Execute');
        }
        else {
            return executeSync(request, 'Execute');
        }
    }

    /**
        Assign a new owner to a record.
        @method assign
        @param assigneeEntityReference {EntityReference} EntityReference of the new owner
        @param targetEntityReference {EntityReference} EntityReference of the record to reassign
        @return {Promise}
        @example
            var newOwnerId = 'eb9c2796-f08f-4d6f-bb4b-03147cba8578',
                recordId = 'ab5f9132-cdbe-4f88-bea0-ccaabd80a80c',
                ownerEntityReference = new Sonoma.OrgService.EntityReference(newOwnerId, 'team'),
                entityReference = new Sonoma.OrgService.EntityReference(recordId, 'contact');

            Sonoma.OrgService.assign(ownerEntityReference, entityReference)
                .then(function(response) {
                    alert('Record has been reassigned');
                },
                function(error) {
                    alert(error);
                });
            // → 'Record has been reassigned'

    **/
    function assign(assigneeEntityReference, targetEntityReference) {
        return _assignInternal(assigneeEntityReference, targetEntityReference, true);
    }

    /**
        Assign a new owner to a record synchronously.
        @method assignSync
        @param assigneeEntityReference {EntityReference} EntityReference of the new owner
        @param targetEntityReference {EntityReference} EntityReference of the record to reassign
        @return {Object} Result object
        @example
            var newOwnerId = 'eb9c2796-f08f-4d6f-bb4b-03147cba8578',
            recordId = 'ab5f9132-cdbe-4f88-bea0-ccaabd80a80c',
            ownerEntityReference = new Sonoma.OrgService.EntityReference(newOwnerId, 'team'),
            entityReference = new Sonoma.OrgService.EntityReference(recordId, 'contact');

            var result = Sonoma.OrgService.assignSync(ownerEntityReference, entityReference);

            if (result.Success) {
                alert('Record has been reassigned');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }

            // → 'Record has been reassigned'
    **/
    function assignSync(assigneeEntityReference, targetEntityReference) {
        return _assignInternal(assigneeEntityReference, targetEntityReference, false);
    }

    function _associateInternal(targetEntityReference, relatedEntitiesArray, relationShip, isAsync) {
        var relatedEntities = '',
            relatedEntitiesLength = relatedEntitiesArray.length,
            request,
            i = 0;

        if (relatedEntitiesArray && relatedEntitiesLength) {
            for (; i < relatedEntitiesLength; i++) {
                relatedEntities += [
                    '<a:EntityReference>',
                        '<a:Id>', relatedEntitiesArray[i].Id, '</a:Id>',
                        '<a:LogicalName>', relatedEntitiesArray[i].LogicalName, '</a:LogicalName>',
                        '<a:Name i:nil="true" />',
                    '</a:EntityReference>'
                ].join('');
            }
        }
        else {
            relatedEntities = [
                    '<a:EntityReference>',
                        '<a:Id>', relatedEntitiesArray.Id, '</a:Id>',
                        '<a:LogicalName>', relatedEntitiesArray.LogicalName, '</a:LogicalName>',
                        '<a:Name i:nil="true" />',
                    '</a:EntityReference>'
            ].join('');
        }

        request = [
                '<request i:type="a:AssociateRequest" xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts">',
                    '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>Target</b:key>',
                            '<b:value i:type="a:EntityReference">',
                                '<a:Id>', targetEntityReference.Id, '</a:Id>',
                                '<a:LogicalName>', targetEntityReference.LogicalName, '</a:LogicalName>',
                                '<a:Name i:nil="true" />',
                            '</b:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>Relationship</b:key>',
                            '<b:value i:type="a:Relationship">',
                                '<a:PrimaryEntityRole i:nil="true" />',
                                '<a:SchemaName>', relationShip, '</a:SchemaName>',
                            '</b:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>RelatedEntities</b:key>',
                            '<b:value i:type="a:EntityReferenceCollection">',
                                relatedEntities,
                            '</b:value>',
                        '</a:KeyValuePairOfstringanyType>',
                    '</a:Parameters>',
                    '<a:RequestId i:nil="true" />',
                    '<a:RequestName>Associate</a:RequestName>',
                '</request>'
        ].join('');

        if (isAsync) {
            return execute(request, 'Execute');
        }
        else {
            return executeSync(request, 'Execute');
        }
    }

    /**
        Associate one or more records to another record, using the CRM relationship name.
        @method associate
        @param targetEntityReference {EntityReference} EntityReference which records will be associated to.
        @param relatedEntities {Array} Array of one or more EntityReferences to be associated to the target.
        @param relationship {String} Name of the relationship for the association.
        @return {Promise}
        @example
            var targetId = 'ec852520-5fc8-44a4-b2a1-ceb4c31801e7',
                relatedId = '975b2900-4116-4ebf-a14e-a335af35e20c',
                target = new Sonoma.OrgService.EntityReference(targetId, 'contact'),
                relatedEntities = [];
            
            relatedEntities.push(new Sonoma.OrgService.EntityReference(relatedId, 'account'));
            
            Sonoma.OrgService.associate(target, relatedEntities, 'account_primary_contact');
                .then(function success(result) {
                    alert('Record has been associated');
                },
                function failure(error) {
                    alert(error);
                });
            // → 'Record has been associated'
    **/
    function associate(targetEntityReference, relatedEntitiesArray, relationShip) {
        return _associateInternal(targetEntityReference, relatedEntitiesArray, relationShip, true);
    }

    /**
        Associate one or more records to another record synchronously, using the CRM relationship name.
        @method associateSync
        @param targetEntityReference {EntityReference} EntityReference which records will be associated to.
        @param relatedEntities {Array} Array of one or more EntityReferences to be associated to the target.
        @param relationship {String} Name of the relationship for the association.
        @return {Object} Result object.
        @example
            var targetId = 'ec852520-5fc8-44a4-b2a1-ceb4c31801e7',
                relatedId = '975b2900-4116-4ebf-a14e-a335af35e20c',
                target = new Sonoma.OrgService.EntityReference(targetId, 'contact'),
                relatedEntities = [];
            
            relatedEntities.push(new Sonoma.OrgService.EntityReference(relatedId, 'account'));
            
            var result = Sonoma.OrgService.associateSync(target, relatedEntities, 'account_primary_contact')
            if(result.Success) {
                alert('Record has been associated');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Record has been associated'
    **/
    function associateSync(targetEntityReference, relatedEntitiesArray, relationShip) {
        return _associateInternal(targetEntityReference, relatedEntitiesArray, relationShip, false);
    }

    function _disassociateInternal(targetEntityReference, relatedEntitiesArray, relationShip, isAsync) {
        var relatedEntities = '',
            relatedEntitiesLength = relatedEntitiesArray.length,
            request,
            i = 0;

        if (relatedEntitiesArray && relatedEntitiesLength) {
            for (; i < relatedEntitiesLength; i++) {
                relatedEntities += [
                    '<a:EntityReference>',
                        '<a:Id>', relatedEntitiesArray[i].Id, '</a:Id>',
                        '<a:LogicalName>', relatedEntitiesArray[i].LogicalName, '</a:LogicalName>',
                        '<a:Name i:nil="true" />',
                    '</a:EntityReference>'
                ].join('');
            }
        }
        else {
            relatedEntities = [
                    '<a:EntityReference>',
                        '<a:Id>', relatedEntitiesArray.Id, '</a:Id>',
                        '<a:LogicalName>', relatedEntitiesArray.LogicalName, '</a:LogicalName>',
                        '<a:Name i:nil="true" />',
                    '</a:EntityReference>'
            ].join('');
        }

        request = [
                '<request i:type="a:DisassociateRequest" xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts">',
                    '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>Target</b:key>',
                            '<b:value i:type="a:EntityReference">',
                                '<a:Id>', targetEntityReference.Id, '</a:Id>',
                                '<a:LogicalName>', targetEntityReference.LogicalName, '</a:LogicalName>',
                                '<a:Name i:nil="true" />',
                            '</b:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>Relationship</b:key>',
                            '<b:value i:type="a:Relationship">',
                                '<a:PrimaryEntityRole i:nil="true" />',
                                '<a:SchemaName>', relationShip, '</a:SchemaName>',
                            '</b:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<b:key>RelatedEntities</b:key>',
                            '<b:value i:type="a:EntityReferenceCollection">',
                                relatedEntities,
                            '</b:value>',
                        '</a:KeyValuePairOfstringanyType>',
                    '</a:Parameters>',
                    '<a:RequestId i:nil="true" />',
                    '<a:RequestName>Disassociate</a:RequestName>',
                '</request>'
        ].join('');

        if (isAsync) {
            return execute(request, 'Execute');
        }
        else {
            return executeSync(request, 'Execute');
        }
    }

    /**
        Remove the relationship between one or more records and another record, using the CRM relationship name.
        @method disassociate
        @param targetEntityReference {EntityReference} EntityReference which will have the relationships removed.
        @param relatedEntities {Array} Array of one or more EntityReferences to be disassociated from the target.
        @param relationship {String} Name of the relationship for the association.
        @return {Promise}
        @example
            var targetId = 'ec852520-5fc8-44a4-b2a1-ceb4c31801e7',
                relatedId = '975b2900-4116-4ebf-a14e-a335af35e20c',
                target = new Sonoma.OrgService.EntityReference(targetId, 'contact'),
                relatedEntities = [];
            
            relatedEntities.push(new Sonoma.OrgService.EntityReference(relatedId, 'account'));
            
            Sonoma.OrgService.disassociate(target, relatedEntities, 'account_primary_contact');
                .then(function success(result) {
                    alert('Record has been disassociated');
                },
                function failure(error) {
                    alert(error);
                });
            // → 'Record has been disassociated'
    **/
    function disassociate(targetEntityReference, relatedEntitiesArray, relationShip) {
        return _disassociateInternal(targetEntityReference, relatedEntitiesArray, relationShip, true);
    }

    /**
        Remove the relationship between one or more records and another record synchronously, using the CRM relationship name.
        @method disassociateSync
        @param targetEntityReference {EntityReference} EntityReference which will have the relationships removed.
        @param relatedEntities {Array} Array of one or more EntityReferences to be disassociated from the target.
        @param relationship {String} Name of the relationship for the association.
        @return {Object} Result object.
        @example
            var targetId = 'ec852520-5fc8-44a4-b2a1-ceb4c31801e7',
                relatedId = '975b2900-4116-4ebf-a14e-a335af35e20c',
                target = new Sonoma.OrgService.EntityReference(targetId, 'contact'),
                relatedEntities = [];
            
            relatedEntities.push(new Sonoma.OrgService.EntityReference(relatedId, 'account'));
            
            var result = Sonoma.OrgService.disassociateSync(target, relatedEntities, 'account_primary_contact')
            if(result.Success) {
                alert('Record has been disassociated');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Record has been disassociated'
    **/
    function disassociateSync(targetEntityReference, relatedEntitiesArray, relationShip) {
        return _disassociateInternal(targetEntityReference, relatedEntitiesArray, relationShip, false);
    }

    function _executeWorkflowRequestInternal(entityId, workflowId, isAsync) {
        var request = [
                '<request i:type="b:ExecuteWorkflowRequest" xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts" xmlns:b="http://schemas.microsoft.com/crm/2011/Contracts">',
                    '<a:Parameters xmlns:c="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                        '<a:KeyValuePairOfstringanyType>',
                            '<c:key>EntityId</c:key>',
                            '<c:value i:type="d:guid" xmlns:d="http://schemas.microsoft.com/2003/10/Serialization/">', entityId, '</c:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<c:key>WorkflowId</c:key>',
                            '<c:value i:type="d:guid" xmlns:d="http://schemas.microsoft.com/2003/10/Serialization/">', workflowId, '</c:value>',
                        '</a:KeyValuePairOfstringanyType>',
                    '</a:Parameters>',
                    '<a:RequestId i:nil="true" />',
                    '<a:RequestName>ExecuteWorkflow</a:RequestName>',
                '</request>'
        ].join('');

        if (isAsync) {
            return execute(request, 'Execute', _parseExecuteWorkflowRequest);
        } else {
            return executeSync(request, 'Execute', _parseExecuteWorkflowRequest);
        }
    }

    /**
        Execute a workflow.
        @method executeWorkflow
        @param entityId {String} Guid of the entity to run the workflow for
        @param workflowId {String} Guid of the workflow to execute
        @return {Promise}
        @example
            var entityId = 'fbb58991-dc10-4547-afdf-15c3a4a70bb4',
            workflowId = '1195c13f-18cd-4642-988d-26977a8b16a4';

            Sonoma.OrgService.executeWorkflow(entityId, workflowId)
                .then(function(systemJobId) {
                    alert('System Job created with Id: ' + systemJobId);
                },
                function(error) {
                    alert(error);
                });
            // → 'System Job created with Id: d5c36ba9-2be7-4828-bf3f-c9acc671d0dd'
    **/
    function executeWorkflow(entityId, workflowId) {
        return _executeWorkflowRequestInternal(entityId, workflowId, true);
    }

    /**
        Execute a workflow synchronously.
        @method executeWorkflowSync
        @param entityId {String} Guid of the entity to run the workflow for
        @param workflowId {String} Guid of the workflow to execute
        @return {Object}
        @example
            var entityId = 'fbb58991-dc10-4547-afdf-15c3a4a70bb4',
                workflowId = '1195c13f-18cd-4642-988d-26977a8b16a4';

            var result = Sonoma.OrgService.executeWorkflowSync(entityId, workflowId);

            if (result.Success) {
                alert('System Job created with Id: ' + result.Value);
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'System Job created with Id: d5c36ba9-2be7-4828-bf3f-c9acc671d0dd'
    **/
    function executeWorkflowSync(entityId, workflowId) {
        return _executeWorkflowRequestInternal(entityId, workflowId, false);
    }

    function _initializeFromRequestInternal(sourceEntityReference, targetLogicalName, targetFieldType, isAsync) {
        if (!targetFieldType || /\S/.test(targetFieldType)) {
            targetFieldType = 'All';
        }

        var request = [
                '<request i:type="b:InitializeFromRequest" xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts" xmlns:b="http://schemas.microsoft.com/crm/2011/Contracts">',
                    '<a:Parameters xmlns:c="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                        '<a:KeyValuePairOfstringanyType>',
                            '<c:key>EntityMoniker</c:key>',
                            '<c:value i:type="a:EntityReference">',
                                '<a:Id>', sourceEntityReference.Id, '</a:Id>',
                                '<a:LogicalName>', sourceEntityReference.LogicalName, '</a:LogicalName>',
                                '<a:Name i:nil="true" />',
                            '</c:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<c:key>TargetEntityName</c:key>',
                            '<c:value i:type="d:string" xmlns:d="http://www.w3.org/2001/XMLSchema">', targetLogicalName, '</c:value>',
                        '</a:KeyValuePairOfstringanyType>',
                        '<a:KeyValuePairOfstringanyType>',
                            '<c:key>TargetFieldType</c:key>',
                            '<c:value i:type="b:TargetFieldType">', targetFieldType, '</c:value>',
                        '</a:KeyValuePairOfstringanyType>',
                    '</a:Parameters>',
                    '<a:RequestId i:nil="true" />',
                    '<a:RequestName>InitializeFrom</a:RequestName>',
                '</request>'
        ].join('');

        if (isAsync) {
            return execute(request, 'Execute', _parseInitializeFromRequest);
        }
        else {
            return executeSync(request, 'Execute', _parseInitializeFromRequest);
        }
    }

    /**
        Initialize a new record from an existing one, using mappings defined in CRM.
        @method initializeFromRequest
        @param sourceEntityReference {EntityReference} Existing record to map from
        @param targetLogicalName {String} Logical name of the entity to create
        @param [targetFieldType='All'] {String} String to filter which fields are mapped. Valid filters are: 'All', 'ValidForCreate', 'ValidForRead' and 'ValidForUpdate'
        @return {Promise}
        @example
            var sourceId = '7b29e013-b82d-40bf-8f96-4387f21b82e5',
                source = new Sonoma.OrgService.EntityReference(sourceId, 'lead');
            
            Sonoma.OrgService.initializeFromRequest(source, 'opportunity', 'All')
                .then(function success(response) {
                    alert('New opportunity initialized from lead');
                },
                function failure(error) {
                    alert(error);
                });
            // → 'New opportunity initialized from lead'
    **/
    function initializeFromRequest(sourceEntityReference, targetLogicalName, targetFieldType) {
        return _initializeFromRequestInternal(sourceEntityReference, targetLogicalName, targetFieldType, true);
    }

    /**
        Synchronously initialize a new record from an existing one, using mappings defined in CRM.
        @method initializeFromRequestSync
        @param sourceEntityReference {EntityReference} Existing record to map from
        @param targetLogicalName {String} Logical name of the entity to create
        @param [targetFieldType='All'] {String} String to filter which fields are mapped. Valid filters are: 'All', 'ValidForCreate', 'ValidForRead' and 'ValidForUpdate'
        @return {Object}
        @example
            var sourceId = '7b29e013-b82d-40bf-8f96-4387f21b82e5',
                source = new Sonoma.OrgService.EntityReference(sourceId, 'lead');
            
            var result = Sonoma.OrgService.initializeFromRequestSync(source, 'opportunity', 'All');
            if(result.Success) {
                alert('New opportunity initialized from lead');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'New opportunity initialized from lead'
    **/
    function initializeFromRequestSync(sourceEntityReference, targetLogicalName, targetFieldType) {
        return _initializeFromRequestInternal(sourceEntityReference, targetLogicalName, targetFieldType, false);
    }

    //#endregion Extension

    function _deleteRecordInternal(logicalName, id, isAsync) {
        var request;

        request = [
                '<entityName>', logicalName, '</entityName>',
                '<id>', id, '</id>'
            ].join('');

        if (isAsync) {
            return execute(request, 'Delete');
        }
        else {
            return executeSync(request, 'Delete');
        }
    }

    /**
        Delete an entity in CRM.
        @method deleteRecord
        @param logicalName {String} LogicalName of the entity
        @param id {String} Guid of the entity
        @return {Promise}
        @example
            var leadId = 'cca4ceb5-6869-41b0-a632-2fea0634ecdf';

            Sonoma.OrgService.deleteRecord('lead', leadId)
                .then(function (data) {
                    alert('Record deleted');
                },
                function (error) {
                    alert(error);
                });
            // → 'Record deleted'
    **/
    function deleteRecord(logicalName, id) {
        return _deleteRecordInternal(logicalName, id, true);
    }

    /**
        Delete an entity in CRM synchronously.
        @method deleteRecordSync
        @param logicalName {String} LogicalName of the entity
        @param id {String} Guid of the entity
        @return {Object}
        @example
            var leadId = 'cca4ceb5-6869-41b0-a632-2fea0634ecdf';

            var result = Sonoma.OrgService.deleteRecordSync('lead', leadId);
            if (result.Success) {
                alert('Record deleted');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Record deleted'
    **/
    function deleteRecordSync(logicalName, id) {
        return _deleteRecordInternal(logicalName, id, false);
    }

    function _createInternal(logicalName, entity, isAsync) {
        var request;

        request = _buildEntityXml(logicalName, null, entity);

        if (isAsync) {
            return execute(request, 'Create', _parseCreate);
        }
        else {
            return executeSync(request, 'Create', _parseCreate);
        }
    }

    /**
        Create a new entity in CRM.
        @method create
        @param logicalName {String} LogicalName of the entity
        @param entity {Object} The entity to be created
        @return {Promise} Promise to return the id of the created entity
        @example
            // Javascript object with CRM attribute logical names as properties
            var newAccount = {
                name: 'New Account',
                accountnumber: '1234',
                telephone1: '+1 (312) 555-0123'
            };

            Sonoma.OrgService.create('account', newAccount)
                .then(function (id) {
                    // 'id' is generated by CRM
                    alert(id);
                },
                function (error) {
                    alert(error);
                });
            // → 'f05edb45-dc67-4df4-ba48-8930a6532360'

    **/
    function create(logicalName, entity) {
        return _createInternal(logicalName, entity, true);
    }

    /**
        Create a new entity in CRM synchronously.
        @method createSync
        @param logicalName {String} LogicalName of the entity
        @param entity {Object} The entity to be created
        @return {String} Guid of created entity.
        @example
            // Javascript object with CRM attribute logical names as properties
            var newAccount = {
                name: 'New Account',
                accountnumber: '1234',
                telephone1: '+1 (312) 555-0123'
            };

            var result = Sonoma.OrgService.createSync('account', newAccount)
            if (result.Success) {
                alert(result.Value);
            }
            // → 'f05edb45-dc67-4df4-ba48-8930a6532360'
    **/
    function createSync(logicalName, entity) {
        return _createInternal(logicalName, entity, false);
    }

    function _updateInternal(logicalName, id, entity, isAsync) {
        var request;

        request = _buildEntityXml(logicalName, id, entity);

        if (isAsync) {
            return execute(request, 'Update');
        }
        else {
            return executeSync(request, 'Update');
        }
    }

    /**
        Update an existing entity in CRM.
        @method update
        @param logicalName {String} LogicalName of the entity
        @param id {String} Guid of entity updated
        @param entity {Object} The entity to be updated
        @return {Object} Promise to return the id of the updated entity
        @example
            var updateAccountId = 'f05edb45-dc67-4df4-ba48-8930a6532360';
    
            // Javascript object with CRM properties to update
            var updateAccount = {
                name: 'Updated Account',
                description: 'I\'m afraid the deflector shield will be quite operational when your friends arrive.'
            };
    
            Sonoma.OrgService.update('account', updateAccountId, updateAccount)
                .then(function success(response) {
                    alert('Updated record with id: ' + updateAccountId);
                },
                function failure(error) {
                    alert(error);
                }
            );
            // → 'Updated record with id: f05edb45-dc67-4df4-ba48-8930a6532360'
    **/
    function update(logicalName, id, entity) {
        return _updateInternal(logicalName, id, entity, true);
    }

    /**
        Update an existing entity in CRM synchronously.
        @method updateSync
        @param logicalName {String} LogicalName of the entity
        @param id {String} Guid of entity updated
        @param entity {Object} The entity fields to be updated
        @return {String} Guid of updated entity.
        @example
            var updateAccountId = 'f05edb45-dc67-4df4-ba48-8930a6532360';

            // Javascript object with CRM properties to update
            var updateAccount = {
                name: 'Update Account',
                description: 'It seems he\'s drowned, selling England by the pound.'
            };
    
            var result = Sonoma.OrgService.updateSync('account', updateAccountId, updateAccount)
            if (result.Success) {
                alert(result.Value);
            }
            // → 'f05edb45-dc67-4df4-ba48-8930a6532360'
    **/
    function updateSync(logicalName, id, entity) {
        return _updateInternal(logicalName, id, entity, false);
    }

    function _retrieveInternal(logicalName, id, fields, isAsync) {
        var request;

        request = [
                '<entityName>', logicalName, '</entityName>',
                '<id>', id, '</id>'
            ];

        request.push(_buildColumnSet(fields));

        if (isAsync) {
            return execute(request.join(''), 'Retrieve', function (responseXML) {
                return _parseRetrieve(responseXML, fields);
            });
        }
        else {
            return executeSync(request.join(''), 'Retrieve', _parseRetrieve);
        }
    }

    /**
        Retrive an entity in CRM.
        @method retrieve
        @param logicalName {String} LogicalName of the entity
        @param id {String} Guid of entity updated
        @param fields {Array} Attributes to retrieve
        @return {Promise} Promise to return the entity data
        @example
            var contactId = 'd69c3a81-dab8-447f-b87a-5fcfafb12081',
                columns = ['firstname', 'lastname'];
    
            Sonoma.OrgService.retrieve('contact', contactId, columns)
                .then(function success(entity) {
                    alert(entity.firstname + ' ' + entity.lastname);
                },
                function failure(error) {
                    alert(error);
                });
            // → 'John Smith'
    **/
    function retrieve(logicalName, id, fields) {
        return _retrieveInternal(logicalName, id, fields, true);
    }

    /**
        Retrive an entity in CRM synchronously.
        @method retrieveSync
        @param logicalName {String} LogicalName of the entity
        @param id {String} Guid of entity to retrieve
        @param fields {Array} Attributes to retrieve
        @return {Object} Result object containing the entity
        @example
            var contactId = 'd69c3a81-dab8-447f-b87a-5fcfafb12081',
                columns = ['statecode', 'statuscode'],
                contact = null;
    
            var result = Sonoma.OrgService.retrieveSync('contact', contactId, columns);
    
            if (result.Success) {
                contact = result.Value;
                alert('Contact has status of: ' + contact.statuscode);
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Contact has status of: 1'
    **/
    function retrieveSync(logicalName, id, fields) {
        return _retrieveInternal(logicalName, id, fields, false);
    }

    function _retrieveMultipleInternal(fetchXml, isAsync) {
        var request;

        fetchXml = Sonoma.Xml.xmlEncode(fetchXml);

        request = [
                '<query i:type="a:FetchExpression" ', _namespaces.xrm, '>',
                    '<a:Query>',
                        fetchXml,
                    '</a:Query>',
                '</query>'
            ].join('');

        if (isAsync) {
            return execute(request, 'RetrieveMultiple', _parseRetrieveMultiple);
        }
        else {
            return executeSync(request, 'RetrieveMultiple', _parseRetrieveMultiple);
        }
    }

    /**
        Retrive multiple entities in CRM.
        @method retrieveMultiple
        @param fetchXml {String} The FetchXML request
        @return {Promise} Promise to return the entities data
        @example
            var fetchXml =
                    ['<fetch mapping="logical" count="100" version="1.0">',
                        '<entity name="systemuser">',
                            '<attribute name="fullname" />',
                        '</entity>',
                    '</fetch>'].join('');
    
            Sonoma.OrgService.retrieveMultiple(fetchXml)
                .then(function success(result) {
                    alert('Returned ' + result.Entities.length + ' results');
                },
                function failure(error) {
                    alert(error);
                });
            // → 'Returned 100 results'
    **/
    function retrieveMultiple(fetchXml) {
        return _retrieveMultipleInternal(fetchXml, true);
    }

    /**
        Retrive multiple entities in CRM synchronously.
        @method retrieveMultipleSync
        @param fetchXml {String} The FetchXML request
        @return {Object} The entities data
        @example
            var fetchXml =
                    ['<fetch mapping="logical" count="100" version="1.0">',
                        '<entity name="systemuser">',
                            '<attribute name="fullname" />',
                        '</entity>',
                    '</fetch>'].join('');
    
            var result = Sonoma.OrgService.retrieveMultipleSync(fetchXml);
            if (result.Success) {
                alert('Returned ' + result.Entities.length + ' results');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Returned 100 results'
    **/
    function retrieveMultipleSync(fetchXml) {
        return _retrieveMultipleInternal(fetchXml, false);
    }

    function _setStateInternal(entityReference, state, status, isAsync) {
        var parameters, request;

        if (Sonoma.type(state) === 'number') {
            state = new Sonoma.OrgService.OptionSetValue(state);
        }
        if (Sonoma.type(status) === 'number') {
            status = new Sonoma.OrgService.OptionSetValue(status);
        }

        parameters = [
                _buildKeyValueXmlPair('EntityMoniker', entityReference.toXml()),
                _buildKeyValueXmlPair('State', state.toXml()),
                _buildKeyValueXmlPair('Status', status.toXml())
            ].join('');

        request = _buildGenericRequest('SetStateRequest', 'SetState', parameters);

        if (isAsync) {
            return execute(request, 'Execute');
        }
        else {
            return executeSync(request, 'Execute');
        }
    }

    /**
        Set an entity's state in CRM.
        @method setState
        @param entityReference {EntityReference} EntityReference of entity
        @param state {Number} State being set
        @param status {Number} Status being set
        @return {Promise} Promise of setState result
        @example
            var leadId = 'fbb58991-dc10-4547-afdf-15c3a4a70bb4',
                leadReference = new Sonoma.OrgService.EntityReference(leadId, 'lead'),
                stateValue = 2, // Disqualified
                statusValue = 6; // No longer interested
    
            Sonoma.OrgService.setState(leadReference, stateValue, statusValue)
                .then(function success(response) {
                    alert('Lead has been disqualified');
                },
                function failure(error) {
                    alert(error);
                });
            // → 'Lead has been disqualified'
    **/
    function setState(entityReference, state, status) {
        return _setStateInternal(entityReference, state, status, true);
    }

    /**
        Set an entity's state in CRM synchrnously.
        @method setStateSync
        @param entityReference {EntityReference} EntityReference of entity
        @param state {Number} State being set
        @param status {Number} Status being set
        @return {Object} Result of setState
        @example
            var leadId = 'fbb58991-dc10-4547-afdf-15c3a4a70bb4',
                leadReference = new Sonoma.OrgService.EntityReference(leadId, 'lead'),
                stateValue = 2, // Disqualified
                statusValue = 6; // No longer interested
    
            var result = Sonoma.OrgService.setStateSync(leadReference, stateValue, statusValue);
            if (result.Success) {
                alert('Lead has been disqualified');
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Lead has been disqualified'
    **/
    function setStateSync(entityReference, state, status) {
        return _setStateInternal(entityReference, state, status, false);
    }
    
    function _executeActionInternal(requestName, inputs, isAsync) {
        var parameters = [],
            request,
            response,
            xmlValue;

        for (var parameter in inputs) {
            if (!inputs.hasOwnProperty(parameter)) {
                continue;
            }

            if (inputs[parameter] == null) {
                xmlValue = new NullValue();
            }
            else if (Sonoma.type(inputs[parameter].toXml) !== 'undefined') {
                xmlValue = inputs[parameter].toXml();
            }
            else if (Sonoma.type(inputs[parameter]) === 'number') {
                xmlValue = [
                    '<b:value i:type="c:int" ', _namespaces.xml, '>',
                        Sonoma.Xml.xmlEncode(inputs[parameter]),
                    '</b:value>'
                ].join('');
            }
            else {
                xmlValue = [
                    '<b:value i:type="c:string" ', _namespaces.xml, '>',
                        Sonoma.Xml.xmlEncode(inputs[parameter]),
                    '</b:value>'
                ].join('');
            }
            parameters.push(_buildKeyValueXmlPair(parameter, xmlValue));
        }

        request = [
            '<request ', _namespaces.xrm, ' ', _namespaces.crm, '>',
                '<a:Parameters ', _namespaces.collection, '>',
                    parameters.join(''),
                '</a:Parameters>',
                '<a:RequestId i:nil="true" />',
                '<a:RequestName>', requestName, '</a:RequestName>',
            '</request>'
        ].join('');

        if (isAsync) {
            return execute(request, 'Execute', function (responseXML) {
                response = {};

                _parseAttributeCollection(response, 
                    responseXML.querySelector('Envelope > Body > ExecuteResponse > ExecuteResult > Results'));

                return response;
            });
        }
        else {
            return executeSync(request, 'Execute', function (responseXML) {
                response = {};

                _parseAttributeCollection(response, 
                    responseXML.querySelector('Envelope > Body > ExecuteResponse > ExecuteResult > Results'));

                return response;
            });
        }
    }

    /**
        Execute a Custom Action in CRM.
        @method executeAction
        @param requestName {String} The custom action's logical name
        @param parameters {Object} Dictionary of Sonoma.OrgService attribute types
        @return {Promise} Promise to return the result of the action
        @example
            var requestName = 'sonoma_FindRelativeAction',
                parameters = {
                    Target: new Sonoma.OrgService.EntityReference('fbb58991-dc10-4547-afdf-15c3a4a70bb4', 'contact')
                };
    
            Sonoma.OrgService.executeAction(requestName, parameters)
                .then(function success(response) {
                    alert(response.Relative.Id);
                },
                function failure(error) {
                    alert(error);
                });
            // → 'Smith Wellingsworth'
    **/
    function executeAction(requestName, params) {
        return _executeActionInternal(requestName, params, true);
    }

    /**
        Execute a Custom Action in CRM synchronously.
        @method executeActionSync
        @param parameters {Object} Dictionary of Sonoma.OrgService attribute types
        @return {Object} Result containing the value returned by the action
        @example
            var requestName = 'sonoma_FindRelativeAction',
                parameters = {
                    Target: new Sonoma.OrgService.EntityReference('fbb58991-dc10-4547-afdf-15c3a4a70bb4', 'contact')
                };
    
            var result = Sonoma.OrgService.executeActionSync(requestName, parameters);
            if (result.Success) {
                alert(result.Value.Relative.Id);
            } else {
                // Value will contain the exception
                alert(result.Value);
            }
            // → 'Smith Wellingsworth'
    **/
    function executeActionSync(requestName, params) {
        return _executeActionInternal(requestName, params, false);
    }

    function _executeInternal(body, requestType, parser, isAsync) {
        var request, req, syncResult, deferred;

        if (_isUndefined(isAsync)) {
            isAsync = true;
        }

        request = [
            '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">',
                '<s:Body>',
                    '<', requestType, ' xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services"',
                        ' xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
                        body,
                    '</', requestType, '>',
                '</s:Body>',
            '</s:Envelope>'
        ].join('');

        req = new XMLHttpRequest();
        req.open('POST', Sonoma.getClientUrl() + '/XRMServices/2011/Organization.svc/web', isAsync);

        // Responses will return XML. It isn't possible to return JSON.
        req.setRequestHeader('Accept', 'application/xml, text/xml, */*');
        req.setRequestHeader('Content-Type', 'text/xml; charset=utf-8');
        req.setRequestHeader('SOAPAction', 'http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/' + requestType);

        if (isAsync) {
            deferred = Sonoma.Promise.defer();

            req.onreadystatechange = function () {
                _handleResponse(req, parser, deferred.resolve, deferred.reject);
            };

            req.send(request);

            return deferred.promise;
        }
        else {
            req.send(request);

            _handleResponse(req, parser,
                function onExecuteSyncSuccess(r) {
                    syncResult = _getReturnObject(true, r);
                },
                function onExecuteSyncFailure(ex) {
                    syncResult = _getReturnObject(false, ex);
                });

            return syncResult;
        }
    }

    /**
        Execute a request in CRM.
        @method execute
        @param body {String}
        @param requestType {String}
        @param parser
        @return {Promise}
    **/
    function execute(body, requestType, parser) {
        return _executeInternal(body, requestType, parser, true);
    }

    /**
        Execute a request in CRM synchronously.
        @method executeSync
        @param body {String}
        @param requestType {String}
        @param parser
        @return {Object}
    **/
    function executeSync(body, requestType, parser) {
        return _executeInternal(body, requestType, parser, false);
    }

    /**
        Execute a web service request
        @method executeWebService
        @param className {String} The fully qualified logic class name to be executed.
        @param data {Object} Input data to be provided as an argument to the logic class.
        @param [transactional=false] {Boolean} Indicates if the operation should occur within a transaction.
        @param [prefix='sonoma'] {String} Solution prefix for the web service entity customizations.
        @param [entity='webservice'] {String} Entity name for the web service entity, which must have the four custom attributes data, error, logicclassname, and istransactional
        @return {Promise} Promise to return a JSON response containing the web service's data on success or error on failure
        @example
            var className = 'SonomaPartners.Xrm.JavaScript.Plugins.WebService.ExampleWebServiceLogic',
                input = {
                    say: 'hello',
                    times: 5
                },
                transactional = false,
                prefix = 'sonoma';
    
            Sonoma.OrgService.executeWebService(className, input, transactional, prefix)
                .then(function (response) {
                    alert(response);
                }, function (error) {
                    alert(error);
                });
    **/
    function executeWebService(className, data, transactional, prefix, entity) {
        var dataAsJsonString = JSON.stringify(data),
            solutionPrefix = prefix || 'sonoma',
            entityName = entity || 'webservice',
            dataAttribute = solutionPrefix + '_data',
            errorAttribute = solutionPrefix + '_error',
            fetch = [
                '<fetch>',
                    '<entity name="', solutionPrefix, '_', entityName, '">',
                        '<attribute name="', solutionPrefix, '_data" />',
                        '<attribute name="', solutionPrefix, '_error" />',
                        '<filter>',
                            '<condition attribute="', solutionPrefix, '_logicclassname" operator="eq" value="', encodeURIComponent(className), '" />',
                            '<condition attribute="', solutionPrefix, '_data" operator="eq" value="', encodeURIComponent(dataAsJsonString), '" />',
                            '<condition attribute="', solutionPrefix, '_istransactional" operator="eq" value="', !!transactional, '" />',
                        '</filter>',
                    '</entity>',
                '</fetch>'
            ].join('');

        return _retrieveMultipleInternal(fetch, true).then(
            function (result) {
                if (result && result.Entities && result.Entities.length) {
                    var output = '';

                    // should only ever return 1 result
                    if (result.Entities[0][dataAttribute]) {
                        output += result.Entities[0][dataAttribute];
                    } else if (result.Entities[0][errorAttribute] !== null) {
                        throw new Error(result.Entities[0][errorAttribute]);
                    } else {
                        throw new Error('{"Error": "Unexpected response received."}');
                    }

                    output = JSON.parse(output);
                    return output;
                } else {
                    throw new Error('{"Error": "Unexpected response received."}');
                }
            });
    }

    /**
        Execute a synchronous web service request
        @method executeWebServiceSync
        @param className {String} The fully qualified logic class name to be executed.
        @param data {Object} Input data to be provided as an argument to the logic class.
        @param [transactional=false] {Boolean} Indicates if the operation should occur within a transaction.
        @param [prefix='sonoma'] {String} Solution prefix for the web service entity customizations.
        @param [entity='webservice'] {String} Entity name for the web service entity, which must have the four custom attributes data, error, logicclassname, and istransactional
        @return {Object} A JSON response containing the web service's data on success or error on failure
        @example
            var className = 'SonomaPartners.Xrm.JavaScript.Plugins.WebService.ExampleWebServiceLogic',
                input = {
                    say: 'hello',
                    times: 5
                },
                transactional = false,
                prefix = 'sonoma';
    
            var result = Sonoma.OrgService.executeWebServiceSync(className, input, transactional, prefix);
            if (result && result.Success) {
                alert('JSON response: ' + result.Value);
            } else {
                // Value will contain the error
                alert(result.Value);
            }
    **/
    function executeWebServiceSync(className, data, transactional, prefix, entity) {
        var result,
            output = '',
            dataAsJsonString = JSON.stringify(data),
            solutionPrefix = prefix || 'sonoma',
            entityName = entity || 'webservice',
            dataAttribute = solutionPrefix + '_data',
            errorAttribute = solutionPrefix + '_error',
            fetch = [
                '<fetch>',
                    '<entity name="', solutionPrefix, '_', entityName, '">',
                        '<attribute name="', solutionPrefix, '_data" />',
                        '<attribute name="', solutionPrefix, '_error" />',
                        '<filter>',
                            '<condition attribute="', solutionPrefix, '_logicclassname" operator="eq" value="', encodeURIComponent(className), '" />',
                            '<condition attribute="', solutionPrefix, '_data" operator="eq" value="', encodeURIComponent(dataAsJsonString), '" />',
                            '<condition attribute="', solutionPrefix, '_istransactional" operator="eq" value="', !!transactional, '" />',
                        '</filter>',
                    '</entity>',
                '</fetch>'
            ].join('');

        result = _retrieveMultipleInternal(fetch, false);
        if (result && result.Value && result.Value.Entities && result.Value.Entities.length) {
            // should only ever return 1 result
            if (result.Value.Entities[0][dataAttribute]) {
                output += result.Value.Entities[0][dataAttribute];
            } else if (result.Value.Entities[0][errorAttribute] !== null) {
                return _getReturnObject(false, new Error(result.Value.Entities[0][errorAttribute]));
            } else {
                return _getReturnObject(false, new Error('{"Error": "Unexpected response received."}'));
            }

            output = JSON.parse(output);
            return _getReturnObject(true, output);
        } else {
            return _getReturnObject(false, new Error('{"Error": "Unexpected response received."}'));
        }
    }

    //#endregion
    //#region Attribute Classes

    function CRMAttribute() {

    }

    CRMAttribute.prototype.toXml = function () {
        alert('toXml() is not implemented for the object ' + this._internalGetName() + '.');
    };

    CRMAttribute.prototype.toString = function () {
        return 'Not implemented';
    };

    CRMAttribute.subClass = function (obj) {
        var fnRegex;

        fnRegex = /\W*function\s+([\w\$]+)\(/;

        obj.prototype = new CRMAttribute();
        obj.prototype._internalGetName = function () {
            var match;

            match = fnRegex.exec(obj.toString()) || [];
            return match[1] || 'No Name';
        };
    };

    /**
        Creates a value object to set an entity attribute to _null_.
        @class NullValue
        @namespace Sonoma.OrgService
        @constructor
        @example
            var nullValue = new Sonoma.OrgService.NullValue();
    **/
    function NullValue() {
        if (!(this instanceof NullValue)) {
            return new NullValue();
        }
    }

    CRMAttribute.subClass(NullValue);

    NullValue.prototype.toXml = function () {
        return '<b:value i:nil="true" />';
    };
    NullValue.prototype.toString = function () {
        return 'null';
    };

    // Use CrmBoolean because Boolean is a reserved JavaScript type (that should never be used).
    /**
        Creates a value object to set an entity attribute to a _Boolean_.
        @class CrmBoolean
        @namespace Sonoma.OrgService
        @param value {Boolean}
        @param [displayValue] {String}
        @constructor
        @example
            var booleanValue = new Sonoma.OrgService.CrmBoolean(true);
    **/
    function CrmBoolean(value, displayValue) {
        if (!(this instanceof CrmBoolean)) {
            return new CrmBoolean(value, displayValue);
        }

        this._type = Sonoma.OrgService.attributeTypes.Boolean;
        this.Value = value;
        this.DisplayValue = displayValue;
    }

    CRMAttribute.subClass(CrmBoolean);

    CrmBoolean.prototype.toXml = function () {
        var attributeXml;

        attributeXml = [
            '<b:value i:type="c:boolean" ', _namespaces.xml, '>',
                this.Value,
            '</b:value>'
        ].join('');

        return attributeXml;
    };
    CrmBoolean.prototype.toString = function () {
        if (this.DisplayValue) {
            return this.DisplayValue;
        }
        else {
            return this.Value;
        }
    };

    /**
        Creates a value object to set an entity attribute to a _Date_.
        @class DateTime
        @namespace Sonoma.OrgService
        @param value {Date}
        @param [displayValue] {String}
        @param [utc] {Number}
        @constructor
        @example
            var dateValue = new Sonoma.OrgService.DateTime(new Date());
        **/
    function DateTime(value, displayValue, utc) {
        if (!(this instanceof DateTime)) {
            return new DateTime(value, displayValue, utc);
        }

        this._type = Sonoma.OrgService.attributeTypes.DateTime;
        this.Value = value;
        this.DisplayValue = displayValue;
        this.UTC = utc;
    }

    CRMAttribute.subClass(DateTime);

    DateTime.prototype.toXml = function () {
        var attributeXml;

        attributeXml = [
            '<b:value i:type="c:dateTime" ', _namespaces.xml, '>',
                Sonoma.Date.toISOString(this.Value),
            '</b:value>'
        ].join('');

        return attributeXml;
    };
    DateTime.prototype.toString = function () {
        if (this.DisplayValue) {
            return this.DisplayValue;
        }
        else {
            return this.Value;
        }
    };

    /**
        Creates a value object to set an entity attribute to a _Decimal_.
        @class Decimal
        @namespace Sonoma.OrgService
        @param value {Number}
        @param [displayValue] {String}
        @constructor
        @example
            var decimalValue = new Sonoma.OrgService.Decimal(1337);
    **/
    function Decimal(value, displayValue) {
        if (!(this instanceof Decimal)) {
            return new Decimal(value, displayValue);
        }

        this._type = Sonoma.OrgService.attributeTypes.Decimal;
        this.Value = value;
        this.DisplayValue = displayValue;
    }

    CRMAttribute.subClass(Decimal);

    Decimal.prototype.toXml = function () {
        var attributeXml;

        attributeXml = [
            '<b:value i:type="c:decimal" ', _namespaces.xml, '>',
                this.Value,
            '</b:value>'
        ].join('');

        return attributeXml;
    };
    Decimal.prototype.toString = function () {
        if (this.DisplayValue) {
            return this.DisplayValue;
        }
        else {
            return this.Value;
        }
    };

    /**
        Creates a value object to set an entity attribute to a _Double_.
        @class Double
        @namespace Sonoma.OrgService
        @param value {Number}
        @param [displayValue] {String}
        @constructor
        @example
            var doubleValue = new Sonoma.OrgService.Double(1337);
    **/
    function Double(value, displayValue) {
        if (!(this instanceof Double)) {
            return new Double(value, displayValue);
        }

        this._type = Sonoma.OrgService.attributeTypes.Double;
        this.Value = value;
        this.DisplayValue = displayValue;
    }

    CRMAttribute.subClass(Double);

    Double.prototype.toXml = function () {
        var attributeXml;
        
        attributeXml = [
                    '<b:value i:type="c:double" ', _namespaces.xml, '>',
                        this.Value,
                    '</b:value>'
        ].join('');

        return attributeXml;
    };

    Double.prototype.toString = function () {
        if (this.DisplayValue != null) {
            return this.DisplayValue;
        }
        else {
            return this.Value;
        }
    };

    /**
        Creates a value object to set an entity attribute to a _Entity Reference_.
        @class EntityReference
        @namespace Sonoma.OrgService
        @param id {String} Guid of referenced entity
        @param logicalName {String} Logical name of referenced entity
        @param [name] {String}
        @constructor
        @example
            var entityReferenceValue = new Sonoma.OrgService.EntityReference(
                '{00000000-0000-0000-0000-000000000000}', 'contact'
            );
    **/
    function EntityReference(id, logicalName, name) {
        if (!(this instanceof EntityReference)) {
            return new EntityReference(id, logicalName, name);
        }

        this._type = Sonoma.OrgService.attributeTypes.EntityReference;
        this.Id = id;
        this.LogicalName = logicalName;
        this.Name = name;
    }

    CRMAttribute.subClass(EntityReference);

    EntityReference.prototype.toXml = function () {
        var attributeXml;

        attributeXml = [
            '<b:value i:type="a:EntityReference">',
                '<a:Id>' + this.Id + '</a:Id>',
                '<a:LogicalName>' + this.LogicalName + '</a:LogicalName>',
                '<a:Name i:nil="true" />',
            '</b:value>'
        ].join('');

        return attributeXml;
    };
    EntityReference.prototype.toString = function () {
        return this.Name;
    };

    /**
        Creates a value object to set an entity attribute to a _Guid_.
        @class Guid
        @namespace Sonoma.OrgService
        @param value {String}
        @param [displayValue] {String}
        @constructor
        @example
            var guidValue = new Sonoma.OrgService.Guid('{00000000-0000-0000-0000-000000000000}');
    **/
    function Guid(value, displayValue) {
        if (!(this instanceof Guid)) {
            return new Guid(value, displayValue);
        }

        this._type = Sonoma.OrgService.attributeTypes.Guid;
        this.Value = value;
    }

    CRMAttribute.subClass(Guid);

    Guid.prototype.toXml = function () {
        var attributeXml = [
                    '<b:value i:type="c:guid" ', _namespaces.serialization, '>',
                        this.Value,
                    '</b:value>'
                ].join('');
        return attributeXml;
    };
    Guid.prototype.toString = function () {
        return this.Value;
    };

    /**
        Creates a value object to set an entity attribute to a _Money_.
        @class Money
        @namespace Sonoma.OrgService
        @param value {Number} Amount of Money
        @param [displayValue] {String}
        @constructor
        @example
            var moneyValue = new Sonoma.OrgService.Money(1.00);
    **/
    function Money(value, displayValue) {
        if (!(this instanceof Money)) {
            return new Money(value, displayValue);
        }

        this._type = Sonoma.OrgService.attributeTypes.Money;
        this.Value = value;
        this.DisplayValue = displayValue;
    }

    CRMAttribute.subClass(Money);

    Money.prototype.toXml = function () {
        var attributeXml = [
                    '<b:value i:type="a:Money">',
                        '<a:Value>' + this.Value + '</a:Value>',
                    '</b:value>'
                ].join('');
        return attributeXml;
    };
    Money.prototype.toString = function () {
        if (this.DisplayValue) {
            return this.DisplayValue;
        }
        else {
            return this.Value;
        }
    };

    /**
        Creates a value object to set an entity attribute to an _OptionSetValue_.
        @class OptionSetValue
        @namespace Sonoma.OrgService
        @param value {Number}
        @param [label] {String}
        @constructor
        @example
            var optionSetValueValue = new Sonoma.OrgService.OptionSetValue(1);
    **/
    function OptionSetValue(value, label) {
        if (!(this instanceof OptionSetValue)) {
            return new OptionSetValue(value, label);
        }

        this._type = Sonoma.OrgService.attributeTypes.OptionSetValue;
        this.Value = value;
        this.Label = label;
    }

    CRMAttribute.subClass(OptionSetValue);

    OptionSetValue.prototype.toXml = function () {
        var attributeXml = [
                    '<b:value i:type="a:OptionSetValue">',
                        '<a:Value>' + this.Value + '</a:Value>',
                    '</b:value>'
                ].join('');
        return attributeXml;
    };
    OptionSetValue.prototype.toString = function () {
        if (this.Label) {
            return this.Label;
        }
        else {
            return this.Value;
        }
    };

    /**
        Creates a value object to set an entity attribute to an _ActivityPartyArray_.
        @class ActivityPartyArray
        @namespace Sonoma.OrgService
        @param parties {Array} Array of {{#crossLink "Sonoma.OrgService.EntityReference"}}{{/crossLink}}
        @constructor
        @example
            var parties = [
                new Sonoma.OrgService.EntityReference('{00000000-0000-0000-0000-000000000000}', 'contact'),
                new Sonoma.OrgService.EntityReference('{00000000-0000-0000-0000-000000000001}', 'contact'),
                new Sonoma.OrgService.EntityReference('{00000000-0000-0000-0000-000000000002}', 'contact')
            ];
            var activityPartyArrayValue = new Sonoma.OrgService.ActivityPartyArray(parties);
    **/
    function ActivityPartyArray(parties) {
        if (!(this instanceof ActivityPartyArray)) {
            return new ActivityPartyArray(parties);
        }

        this._type = 'ActivityPartyArray';
        this.EntityReferences = parties;
    }

    CRMAttribute.subClass(ActivityPartyArray);

    ActivityPartyArray.prototype.toXml = function () {
        var valueXml = [],
            i = 0,
            len,
            template,
            attributeXml;

        len = this.EntityReferences.length;
        for (i = 0; i < len; i++) {
            template = [
                    '<a:Entity>',
                        '<a:Attributes>',
                          '<a:KeyValuePairOfstringanyType>',
                            '<b:key>partyid</b:key>',
                            '<b:value i:type="a:EntityReference">',
                              '<a:Id>', this.EntityReferences[i].Id, '</a:Id>',
                              '<a:LogicalName>', this.EntityReferences[i].LogicalName, '</a:LogicalName>',
                              '<a:Name i:nil="true" />',
                            '</b:value>',
                          '</a:KeyValuePairOfstringanyType>',
                        '</a:Attributes>',
                        '<a:EntityState i:nil="true" />',
                        '<a:FormattedValues />',
                        '<a:Id>00000000-0000-0000-0000-000000000000</a:Id>',
                        '<a:LogicalName>activityparty</a:LogicalName>',
                        '<a:RelatedEntities />',
                      '</a:Entity>'
                ].join('');
            valueXml.push(template);
        }

        attributeXml = [
                    '<b:value i:type="a:ArrayOfEntity">',
                        valueXml.join(''),
                    '</b:value>'
                ].join('');
        return attributeXml;
    };

    ActivityPartyArray.prototype.toString = function () {
        return '';
    };

    //#endregion
    //#region Public
    return {
        attributeTypes: attributeTypes,
        
        /*-- Service Methods --*/
        create: create,
        createSync: createSync,
        update: update,
        updateSync: updateSync,
        retrieve: retrieve,
        retrieveSync: retrieveSync,
        retrieveMultiple: retrieveMultiple,
        retrieveMultipleSync: retrieveMultipleSync,
        setState: setState,
        setStateSync: setStateSync,
        execute: execute,
        executeSync: executeSync,
        executeAction: executeAction,
        executeActionSync: executeActionSync,
        executeWebService: executeWebService,
        executeWebServiceSync: executeWebServiceSync,
        deleteRecord: deleteRecord,
        deleteRecordSync: deleteRecordSync,

        /*-- Attribute Classes --*/
        NullValue: NullValue,
        Boolean: CrmBoolean,
        DateTime: DateTime,
        Decimal: Decimal,
        Double: Double,
        EntityReference: EntityReference,
        Guid: Guid,
        Money: Money,
        OptionSetValue: OptionSetValue,
        ActivityPartyArray: ActivityPartyArray,

        /*-- Extension --*/
        executeWorkflow: executeWorkflow,
        executeWorkflowSync: executeWorkflowSync,
        initializeFromRequest: initializeFromRequest,
        initializeFromRequestSync: initializeFromRequestSync,
        assign: assign,
        assignSync: assignSync,
        associate: associate,
        associateSync: associateSync,
        disassociate: disassociate,
        disassociateSync: disassociateSync
    };

    //#endregion
}());

/// <reference path='sonoma.js' />
/// <reference path='orgservice.js' />
/*globals Sonoma, Q*/
/**
    Metadata Module
    @module Sonoma
    @submodule Metadata
**/
/**
    Metadata Utilities
    @class Metadata
    @namespace Sonoma
**/
Sonoma.Metadata = (function () {
    //#region Constants

    var _endpoint = '/XRMServices/2011/Organization.svc/web',
        _namespaces = { /* TODO: find and replace all namespaces */
            xrm: 'xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts"',
            crm: 'xmlns:c="http://schemas.microsoft.com/crm/2011/Contracts"',
            collection: 'xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic"',
            arrays: 'xmlns:b="http://schemas.microsoft.com/2003/10/Serialization/Arrays"',
            xml: 'xmlns:c="http://www.w3.org/2001/XMLSchema"',
            serialization: 'xmlns:c="http://schemas.microsoft.com/2003/10/Serialization/"',

            // Metadata
            soapenv: 'xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"',
            xmli: 'xmlns:i="http://www.w3.org/2001/XMLSchema-instance"'
        },
        entityFilters = {
            All: 15, // Entity Attributes Privileges Relationships
            Attributes: 3, // Entity Attributes
            Default: 1, // Entity
            Entity: 1, // Entity
            Privileges: 5, // Entity Privileges
            Relationships: 9 // Entity Relationships
        };

    //#endregion
    //#region Utility Functions

    // Private function that wraps a SOAP envelope around a request
    function _getSOAPWrapper(request) {
        var soap = [
            '<soapenv:Envelope ', _namespaces.soapenv, '>',
              '<soapenv:Body>',
                request,
              '</soapenv:Body>',
            '</soapenv:Envelope>'
        ];

        return soap.join('');
    }

    // Private function that sends a SOAP request to the 2011 SOAP endpoint
    function _sendRequest(soap, parser, async, extraData) {
        if (Sonoma.type(async) === 'undefined') { async = true; }

        var orgSvcUrl = Sonoma.getClientUrl() + _endpoint,
            request = new XMLHttpRequest(),
            deferred;

        request.open('POST', orgSvcUrl, async);
        request.setRequestHeader('Accept', 'application/xml, text/xml, */*');
        request.setRequestHeader('Content-Type', 'text/xml; charset=utf-8');
        request.setRequestHeader('SOAPAction', 'http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute');

        if (async) {
            deferred = Sonoma.Promise.defer();
            request.onreadystatechange = function readyStateChange() {
                parser(request, extraData, deferred.resolve, deferred.reject);
            };
        }

        request.send(soap);

        if (!async) {
            return parser(request, extraData);
        }

        return deferred.promise;
    }

    // Private function that creates a return object for synchronous calls
    function _getReturnObject(returnValue, successFlag) {
        return {
            Success: successFlag,
            Value: returnValue
        };
    }

    // Private function that attempts to parse errors related to connectivity or WCF faults
    // Error descriptions come from http://support.microsoft.com/kb/193625
    function _getError(response) {
        if (response.status === '12029') { /* TODO: Validate this is a string */
            return new Error('The attempt to connect to the server failed.');
        }
        if (response.status === '12007') { /* TODO: Validate this is a string */
            return new Error('The server name could not be resolved.');
        }

        var bodyNode, node, faultStringNode,
            i, iLength, j, jLength,
            faultXml = Sonoma.Xml.loadXml(response.responseText),
            errorMessage = 'Unknown error (unable to parse the fault)';

        if (Sonoma.type(faultXml) === 'object') {
            if (faultXml.firstChild && faultXml.firstChild.firstChild) {
                bodyNode = faultXml.firstChild.firstChild;

                //Retrieve the fault node
                iLength = bodyNode.childNodes.length;
                for (i = 0; i < iLength; i++) {
                    node = bodyNode.childNodes[i];

                    //NOTE: This comparison does not handle the case where the XML namespace changes
                    if ('s:Fault' === node.nodeName) {
                        jLength = node.childNodes.length;
                        for (j = 0; j < jLength; j++) {
                            faultStringNode = node.childNodes[j];
                            if ('faultstring' === faultStringNode.nodeName) {
                                errorMessage = faultStringNode.text || faultStringNode.textContent;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        return new Error(errorMessage);
    }

    function _getChildNode(contextNode, childName) {
        if (!contextNode || !contextNode.hasChildNodes()) {
            return null;
        }
        return contextNode.querySelector(contextNode.localName + ' > ' + childName);
    }

    function _getChildNodeText(contextNode, childName) {
        var node = _getChildNode(contextNode, childName);
        return (node) ? node.textContent : '';
    }

    //#endregion
    //#region EntityFilter Handlers

    // Private function to get EntityFilter string from EntityFilters enum
    function _getEntityFilterString(entityFilters) {
        if (Sonoma.type(entityFilters) === 'string') {
            return entityFilters;
        }

        var filter = 'Entity';

        /*jshint bitwise: false*/
        if (entityFilters & 2) { filter += ' Attributes'; }
        if (entityFilters & 4) { filter += ' Privileges'; }
        if (entityFilters & 8) { filter += ' Relationships'; }
        /*jshint bitwise: true*/

        return filter;
    }

    function _entityFilterStringToEnum(filterString) {
        if (Sonoma.type(filterString) === 'number') {
            return filterString;
        }

        var enumValue = 1;
        if (filterString.indexOf('Attributes') >= 0) {
            enumValue += 2;
        }
        if (filterString.indexOf('Privileges') >= 0) {
            enumValue += 4;
        }
        if (filterString.indexOf('Relationships') >= 0) {
            enumValue += 8;
        }

        return enumValue;
    }

    // Private function that caches an EntityMetadata object
    function _cacheEntityMetadata(entityMetadata, retrievedFilterLevel) {
        var filterLevel = retrievedFilterLevel,
            existingFilterLevel = 0,
            existingCache = Sonoma.Cache.get('Metadata.Entities', entityMetadata.LogicalName),
            existingMetadata,
            filterDifference,
            cacheObject;

        if (existingCache) {
            existingFilterLevel = existingCache.FilterLevel;
            existingMetadata = existingCache.Metadata;

            /*jshint bitwise: false*/
            filterLevel = retrievedFilterLevel | existingFilterLevel;
            filterDifference = filterLevel ^ retrievedFilterLevel;

            if (filterDifference & 2) { entityMetadata.Attributes = existingMetadata.Attributes; }
            if (filterDifference & 4) { entityMetadata.Privileges = existingMetadata.Privileges; }
            if (filterDifference & 8) {
                entityMetadata.OneToManyRelationships = existingMetadata.OneToManyRelationships;
                entityMetadata.ManyToOneRelationships = existingMetadata.ManyToOneRelationships;
                entityMetadata.ManyToManyRelationships = existingMetadata.ManyToManyRelationships;
            }
            /*jshint bitwise: true*/
        }

        cacheObject = {
            FilterLevel: filterLevel,
            Metadata: entityMetadata
        };

        Sonoma.Cache.set('Metadata.Entities', entityMetadata.LogicalName, cacheObject);
        return existingFilterLevel;
    }

    //#endregion
    //#region OptionSetMetadata Parsing Functions

    // Private function that creates a BooleanOptionSetMetadata object from an XML node
    function _getBooleanOptionSet(node) {
        if (node.childNodes.length === 0) { return null; }
        else {
            return {
                MetadataId: _getChildNodeText(node, 'MetadataId'),
                Description: _getLabelSet(_getChildNode(node, 'Description')),
                DisplayName: _getLabelSet(_getChildNode(node, 'DisplayName')),
                IsCustomOptionSet: _getNullableBoolean(_getChildNode(node, 'IsCustomOptionSet')),
                IsCustomizable: _getBooleanManagedProperty(_getChildNode(node, 'IsCustomizable')),
                IsGlobal: _getNullableBoolean(_getChildNode(node, 'IsGlobal')),
                IsManaged: _getNullableBoolean(_getChildNode(node, 'IsManaged')),
                Name: _getChildNodeText(node, 'Name'),
                OptionSetType: _getChildNodeText(node, 'OptionSetType'),
                FalseOption: {
                    MetadataId: _getChildNodeText(node, 'FalseOption > MetadataId'),
                    Description: _getLabelSet(_getChildNode(node, 'FalseOption > Description')),
                    IsManaged: _getNullableBoolean(_getChildNode(node, 'FalseOption > IsManaged')),
                    Label: _getLabelSet(_getChildNode(node, 'FalseOption > Label')),
                    Value: _getNullableInt(_getChildNode(node, 'FalseOption > Value'))
                },
                TrueOption: {
                    MetadataId: _getChildNodeText(node, 'TrueOption > MetadataId'),
                    Description: _getLabelSet(_getChildNode(node, 'TrueOption > Description')),
                    IsManaged: _getNullableBoolean(_getChildNode(node, 'TrueOption > IsManaged')),
                    Label: _getLabelSet(_getChildNode(node, 'TrueOption > Label')),
                    Value: _getNullableInt(_getChildNode(node, 'TrueOption > Value'))
                }
            };
        }
    }

    // Private function that creates OptionSetMetadata Options from an XML node
    function _getOptions(node) {
        var optionMetadataArray = [],
            optionNode,
            option,
            i, len;

        len = node.childNodes.length;
        for (i = 0; i < len; i++) {
            optionNode = node.childNodes[i];
            option = {
                MetadataId: _getChildNodeText(optionNode, 'MetadataId'),
                Description: _getLabelSet(_getChildNode(optionNode, 'Description')),
                IsManaged: _getNullableBoolean(_getChildNode(optionNode, 'IsManaged')),
                Label: _getLabelSet(_getChildNode(optionNode, 'Label')),
                Value: _getNullableInt(_getChildNode(optionNode, 'Value')),
                State: _getChildNode(optionNode, 'State') ? _getNullableInt(_getChildNode(optionNode, 'State')) : null
            };
            optionMetadataArray.push(option);
        }

        return optionMetadataArray;
    }

    // Private function that creates an OptionSetMetadata object from an XML node
    function GetOptionSetMetadata(node) {
        if (node.childNodes.length === 0) { return null; }
        else {
            return {
                MetadataId: _getChildNodeText(node, 'MetadataId'),
                Description: _getLabelSet(_getChildNode(node, 'Description')),
                DisplayName: _getLabelSet(_getChildNode(node, 'DisplayName')),
                IsCustomOptionSet: _getNullableBoolean(_getChildNode(node, 'IsCustomOptionSet')),
                IsCustomizable: _getBooleanManagedProperty(_getChildNode(node, 'IsCustomizable')),
                IsGlobal: _getNullableBoolean(_getChildNode(node, 'IsGlobal')),
                IsManaged: _getNullableBoolean(_getChildNode(node, 'IsManaged')),
                Name: _getChildNodeText(node, 'Name'),
                OptionSetType: _getChildNodeText(node, 'OptionSetType'),
                Options: _getOptions(_getChildNode(node, 'Options'))
            };
        }
    }

    //#endregion
    //#region Object Parsing Functions

    // Private function that creates an AssociatedMenuConfiguration object from an XML node
    function _getAssociatedMenuConfiguration(node) {
        var orderValue = _getNullableInt(_getChildNode(node, 'Order'));
        if (isNaN(orderValue)) { orderValue = null; }

        return {
            Behavior: _getChildNodeText(node, 'Behavior'),
            Group: _getChildNodeText(node, 'Group'),
            Label: _getLabelSet(_getChildNode(node, 'Label')),
            Order: orderValue
        };
    }

    // Private function that creates a BooleanManagedProperty object from an XML node
    function _getBooleanManagedProperty(node) {
        if (!node || !node.textContent) { return null; }
        return {
            CanBeChanged: _getChildNodeText(node, 'CanBeChanged') === 'true' ? true : false,
            ManagedPropertyLogicalName: _getChildNodeText(node, 'ManagedPropertyLogicalName'),
            Value: _getChildNodeText(node, 'Value') === 'true' ? true : false
        };
    }

    // Private function that creates a label object from an XML node
    function _getLabel(node) {
        if (!node || !node.textContent) { return null; }
        return {
            IsManaged: _getChildNodeText(node, 'IsManaged') === 'true' ? true : false,
            Label: _getChildNodeText(node, 'Label'),
            LanguageCode: _getNullableInt(_getChildNode(node, 'LanguageCode'))
        };
    }

    // Private function that creates a set of labels from an XML node
    function _getLabelSet(node) {
        if (!node || !node.textContent) { return null; }

        var locLabels = _getChildNode(node, 'LocalizedLabels'),
            userLocLabel = _getChildNode(node, 'UserLocalizedLabel'),
            locLabelArray = [],
            locLabelNode,
            i = 0, len;

        if (locLabels) {
            len = locLabels.childNodes.length;
            for (i = 0; i < len; i++) {
                locLabelNode = locLabels.childNodes[i];
                locLabelArray.push(_getLabel(locLabelNode));
            }
        }

        return {
            LocalizedLabels: locLabelArray,
            UserLocalizedLabel: userLocLabel ? _getLabel(userLocLabel) : null
        };
    }

    // Private function that creates a nullable Boolean from an XML node
    function _getNullableBoolean(node) {
        if (!node || !node.textContent) { return null; }
        return node.textContent === 'true' ? true : false;
    }

    // Private function that creates a nullable Integer from an XML node
    function _getNullableInt(node) {
        if (!node || !node.textContent) { return null; }
        return parseInt(node.textContent, 10);
    }

    // Private function that creates an AttributeRequiredLevelManagedProperty from an XML node
    function _getRequiredLevelManagedProperty(node) {
        if (!node || !node.textContent) { return null; }
        return {
            CanBeChanged: _getChildNodeText(node, 'CanBeChanged') === 'true' ? true : false,
            ManagedPropertyLogicalName: _getChildNodeText(node, 'ManagedPropertyLogicalName'),
            Value: _getChildNodeText(node, 'Value')
        };
    }

    //#endregion
    //#region SecurityPrivilegeMetadata Parsing Functions

    // Private function that creates a SecurityPrivilegeMetadata object from an XML node
    function _getSecurityPrivilegeMetadata(node) {
        return {
            CanBeBasic: _getChildNodeText(node, 'CanBeBasic') === 'true' ? true : false,
            CanBeDeep: _getChildNodeText(node, 'CanBeDeep') === 'true' ? true : false,
            CanBeGlobal: _getChildNodeText(node, 'CanBeGlobal') === 'true' ? true : false,
            CanBeLocal: _getChildNodeText(node, 'CanBeLocal') === 'true' ? true : false,
            Name: _getChildNodeText(node, 'Name'),
            PrivilegeId: _getChildNodeText(node, 'PrivilegeId'),
            PrivilegeType: _getChildNodeText(node, 'PrivilegeType')
        };
    }

    //#endregion
    //#region RelationshipMetadata Parsing Functions

    // Private function that creates a OneToManyRelationshipMetadata object from an XML node
    function _getOneToManyRelationshipMetadata(node) {
        return {
            MetadataId: _getChildNodeText(node, 'MetadataId'),
            IsCustomRelationship: _getChildNodeText(node, 'IsCustomRelationship') === 'true' ? true : false,
            IsCustomizable: _getBooleanManagedProperty(_getChildNode(node, 'IsCustomizable')),
            IsManaged: _getChildNodeText(node, 'IsManaged') === 'true' ? true : false,
            IsValidForAdvancedFind: _getChildNodeText(node, 'IsValidForAdvancedFind') === 'true' ? true : false,
            SchemaName: _getChildNodeText(node, 'SchemaName'),
            SecurityTypes: _getChildNodeText(node, 'SecurityTypes'),
            AssociatedMenuConfiguration: _getAssociatedMenuConfiguration(_getChildNode(node, 'AssociatedMenuConfiguration')),
            CascadeConfiguration: {
                Assign: _getChildNodeText(node, 'CascadeConfiguration > Assign'),
                Delete: _getChildNodeText(node, 'CascadeConfiguration > Delete'),
                Merge: _getChildNodeText(node, 'CascadeConfiguration > Merge'),
                Reparent: _getChildNodeText(node, 'CascadeConfiguration > Reparent'),
                Share: _getChildNodeText(node, 'CascadeConfiguration > Share'),
                Unshare: _getChildNodeText(node, 'CascadeConfiguration > Unshare')
            },
            ReferencedAttribute: _getChildNodeText(node, 'ReferencedAttribute'),
            ReferencedEntity: _getChildNodeText(node, 'ReferencedEntity'),
            ReferencingAttribute: _getChildNodeText(node, 'ReferencingAttribute'),
            ReferencingEntity: _getChildNodeText(node, 'ReferencingEntity')
        };
    }

    // Private function that creates a ManyToManyRelationshipMetadata object from an XML node
    function _getManyToManyRelationshipMetadata(node) {
        return {
            MetadataId: _getChildNodeText(node, 'MetadataId'),
            IsCustomRelationship: _getChildNodeText(node, 'IsCustomRelationship') === 'true' ? true : false,
            IsCustomizable: _getBooleanManagedProperty(_getChildNode(node, 'IsCustomizable')),
            IsManaged: _getChildNodeText(node, 'IsManaged') === 'true' ? true : false,
            IsValidForAdvancedFind: _getChildNodeText(node, 'IsValidForAdvancedFind') === 'true' ? true : false,
            SchemaName: _getChildNodeText(node, 'SchemaName'),
            SecurityTypes: _getChildNodeText(node, 'SecurityTypes'),
            Entity1AssociatedMenuConfiguration: _getAssociatedMenuConfiguration(_getChildNode(node, 'Entity1AssociatedMenuConfiguration')),
            Entity1IntersectAttribute: _getChildNodeText(node, 'Entity1IntersectAttribute'),
            Entity1LogicalName: _getChildNodeText(node, 'Entity1LogicalName'),
            Entity2AssociatedMenuConfiguration: _getAssociatedMenuConfiguration(_getChildNode(node, 'Entity2AssociatedMenuConfiguration')),
            Entity2IntersectAttribute: _getChildNodeText(node, 'Entity2IntersectAttribute'),
            Entity2LogicalName: _getChildNodeText(node, 'Entity2LogicalName'),
            IntersectEntityName: _getChildNodeText(node, 'IntersectEntityName')
        };
    }

    //#endregion
    //#region EntityMetadata Parsing Functions

    // Private function that creates an EntityMetadata object from an XML node
    function _getEntityMetadata(node) {
        // Check for Attributes and add them if they are included
        var attributes,
            attributesNode = _getChildNode(node, 'Attributes'),
            attributeNode, attribute,
            privilegesNode, privilegeNode, privileges, privilege,
            oneToManyRelationshipsNode, oneToManyRelationshipNode, oneToManyRelationships, oneToManyRelationship,
            manyToOneRelationshipsNode, manyToOneRelationshipNode, manyToOneRelationships, manyToOneRelationship,
            manyToManyRelationshipsNode, manyToManyRelationshipNode, manyToManyRelationships, manyToManyRelationship,
            i = 0, len;

        if (attributesNode) {
            len = attributesNode.childNodes.length;
            for (i = 0; i < len; i++) {
                if (!attributes) { attributes = {}; }
                attributeNode = attributesNode.childNodes[i];
                attribute = new GetAttributeMetadata(attributeNode);
                attributes[attribute.LogicalName] = attribute;
            }
        }

        //Check for Privileges and add them if they are included
        privilegesNode = _getChildNode(node, 'Privileges');
        if (privilegesNode) {
            len = privilegesNode.childNodes.length;
            for (i = 0; i < len; i++) {
                if (!privileges) { privileges = {}; }
                privilegeNode = privilegesNode.childNodes[i];
                privilege = _getSecurityPrivilegeMetadata(privilegeNode);
                privileges[privilege.Name] = privilege;
            }
        }

        //Check for Relationships and add them if they are included
        oneToManyRelationshipsNode = _getChildNode(node, 'OneToManyRelationships');
        if (oneToManyRelationshipsNode) {
            len = oneToManyRelationshipsNode.childNodes.length;
            for (i = 0; i < len; i++) {
                if (!oneToManyRelationships) { oneToManyRelationships = {}; }
                oneToManyRelationshipNode = oneToManyRelationshipsNode.childNodes[i];
                oneToManyRelationship = _getOneToManyRelationshipMetadata(oneToManyRelationshipNode);
                oneToManyRelationships[oneToManyRelationship.SchemaName] = oneToManyRelationship;
            }
        }

        manyToOneRelationshipsNode = _getChildNode(node, 'ManyToOneRelationships');
        if (manyToOneRelationshipsNode) {
            len = manyToOneRelationshipsNode.childNodes.length;
            for (i = 0; i < len; i++) {
                if (!manyToOneRelationships) { manyToOneRelationships = {}; }
                manyToOneRelationshipNode = manyToOneRelationshipsNode.childNodes[i];
                manyToOneRelationship = _getOneToManyRelationshipMetadata(manyToOneRelationshipNode);
                manyToOneRelationships[manyToOneRelationship.SchemaName] = manyToOneRelationship;
            }
        }

        manyToManyRelationshipsNode = _getChildNode(node, 'ManyToManyRelationships');
        if (manyToManyRelationshipsNode) {
            len = manyToManyRelationshipsNode.childNodes.length;
            for (i = 0; i < len; i++) {
                if (!manyToManyRelationships) { manyToManyRelationships = {}; }
                manyToManyRelationshipNode = manyToManyRelationshipsNode.childNodes[i];
                manyToManyRelationship = _getManyToManyRelationshipMetadata(manyToManyRelationshipNode);
                manyToManyRelationships[manyToManyRelationship.SchemaName] = manyToManyRelationship;
            }
        }

        return {
            ActivityTypeMask: _getNullableInt(_getChildNode(node, 'ActivityTypeMask')),
            Attributes: attributes,
            AutoRouteToOwnerQueue: _getNullableBoolean(_getChildNode(node, 'AutoRouteToOwnerQueue')),
            CanBeInManyToMany: _getBooleanManagedProperty(_getChildNode(node, 'CanBeInManyToMany')),
            CanBePrimaryEntityInRelationship: _getBooleanManagedProperty(_getChildNode(node, 'CanBePrimaryEntityInRelationship')),
            CanBeRelatedEntityInRelationship: _getBooleanManagedProperty(_getChildNode(node, 'CanBeRelatedEntityInRelationship')),
            CanCreateAttributes: _getBooleanManagedProperty(_getChildNode(node, 'CanCreateAttributes')),
            CanCreateCharts: _getBooleanManagedProperty(_getChildNode(node, 'CanCreateCharts')),
            CanCreateForms: _getBooleanManagedProperty(_getChildNode(node, 'CanCreateForms')),
            CanCreateViews: _getBooleanManagedProperty(_getChildNode(node, 'CanCreateViews')),
            CanModifyAdditionalSettings: _getBooleanManagedProperty(_getChildNode(node, 'CanModifyAdditionalSettings')),
            CanTriggerWorkflow: _getNullableBoolean(_getChildNode(node, 'CanTriggerWorkflow')),
            Description: _getLabelSet(_getChildNode(node, 'Description')),
            DisplayCollectionName: _getLabelSet(_getChildNode(node, 'DisplayCollectionName')),
            DisplayName: _getLabelSet(_getChildNode(node, 'DisplayName')),
            IconLargeName: _getChildNodeText(node, 'IconLargeName'),
            IconMediumName: _getChildNodeText(node, 'IconMediumName'),
            IconSmallName: _getChildNodeText(node, 'IconSmallName'),
            IsActivity: _getNullableBoolean(_getChildNode(node, 'IsActivity')),
            IsActivityParty: _getNullableBoolean(_getChildNode(node, 'IsActivityParty')),
            IsAuditEnabled: _getBooleanManagedProperty(_getChildNode(node, 'IsAuditEnabled')),
            IsAvailableOffline: _getNullableBoolean(_getChildNode(node, 'IsAvailableOffline')),
            IsChildEntity: _getNullableBoolean(_getChildNode(node, 'IsChildEntity')),
            IsConnectionsEnabled: _getBooleanManagedProperty(_getChildNode(node, 'IsConnectionsEnabled')),
            IsCustomEntity: _getNullableBoolean(_getChildNode(node, 'IsCustomEntity')),
            IsCustomizable: _getBooleanManagedProperty(_getChildNode(node, 'IsCustomizable')),
            IsDocumentManagementEnabled: _getNullableBoolean(_getChildNode(node, 'IsDocumentManagementEnabled')),
            IsDuplicateDetectionEnabled: _getBooleanManagedProperty(_getChildNode(node, 'IsDuplicateDetectionEnabled')),
            IsEnabledForCharts: _getNullableBoolean(_getChildNode(node, 'IsEnabledForCharts')),
            IsImportable: _getNullableBoolean(_getChildNode(node, 'IsImportable')),
            IsIntersect: _getNullableBoolean(_getChildNode(node, 'IsIntersect')),
            IsMailMergeEnabled: _getBooleanManagedProperty(_getChildNode(node, 'IsMailMergeEnabled')),
            IsManaged: _getNullableBoolean(_getChildNode(node, 'IsManaged')),
            IsMappable: _getBooleanManagedProperty(_getChildNode(node, 'IsMappable')),
            IsReadingPaneEnabled: _getNullableBoolean(_getChildNode(node, 'IsReadingPaneEnabled')),
            IsRenameable: _getBooleanManagedProperty(_getChildNode(node, 'IsRenameable')),
            IsValidForAdvancedFind: _getNullableBoolean(_getChildNode(node, 'IsValidForAdvancedFind')),
            IsValidForQueue: _getBooleanManagedProperty(_getChildNode(node, 'IsValidForQueue')),
            IsVisibleInMobile: _getBooleanManagedProperty(_getChildNode(node, 'IsVisibleInMobile')),
            LogicalName: _getChildNode(node, 'LogicalName').textContent,
            ManyToManyRelationships: manyToManyRelationships,
            ManyToOneRelationships: manyToOneRelationships,
            MetadataId: _getChildNode(node, 'MetadataId').textContent,
            ObjectTypeCode: _getNullableInt(_getChildNode(node, 'ObjectTypeCode')),
            OneToManyRelationships: oneToManyRelationships,
            OwnershipType: _getChildNodeText(node, 'OwnershipType'),
            PrimaryIdAttribute: _getChildNodeText(node, 'PrimaryIdAttribute'),
            PrimaryNameAttribute: _getChildNodeText(node, 'PrimaryNameAttribute'),
            Privileges: privileges,
            RecurrenceBaseEntityLogicalName: _getChildNodeText(node, 'RecurrenceBaseEntityLogicalName'),
            ReportViewName: _getChildNodeText(node, 'ReportViewName'),
            SchemaName: _getChildNodeText(node, 'SchemaName'),
            toString: function () { return this.LogicalName; }
        };
    }

    //#endregion
    //#region OptionSetMetadata Retrieval Functions

    // Private function that creates a SOAP RetrieveGlobalOptionSetRequest
    function _getRetrieveGlobalOptionSetSOAP(optionSetName, retrieveAsIfPublished) {
        var soap = [
            '<Execute xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services" ',
                'xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
              '<request i:type="a:RetrieveOptionSetRequest" ',
                  'xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts">',
                '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>MetadataId</b:key>',
                    '<b:value i:type="ser:guid" xmlns:ser="http://schemas.microsoft.com/2003/10/Serialization/">',
                      '00000000-0000-0000-0000-000000000000',
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>Name</b:key>',
                    '<b:value i:type="c:string" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      optionSetName,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>RetrieveAsIfPublished</b:key>',
                    '<b:value i:type="c:boolean" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      !!retrieveAsIfPublished,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                '</a:Parameters>',
                '<a:RequestId i:nil="true" />',
                '<a:RequestName>RetrieveOptionSet</a:RequestName>',
              '</request>',
            '</Execute>'
        ];

        return _getSOAPWrapper(soap.join(''));
    }

    // Private function that processes a RetrieveGlobalOptionSetResponse
    function _retrieveGlobalOptionSetParser(response, optionSetName, successCallback, errorCallback) {
        var xmlDocument,
            optionSetMetadata;
        if (response.readyState === 4) { // Complete
            if (response.status === 200) { // Success
                xmlDocument = Sonoma.Xml.loadXml(response.responseText);

                optionSetMetadata = new GetOptionSetMetadata(xmlDocument.querySelector('ExecuteResult value'));

                Sonoma.Cache.set('Metadata.GlobalOptionSets', optionSetName, optionSetMetadata);

                successCallback(optionSetMetadata);
            }
            else {
                errorCallback(_getError(response));
            }
        }

        response = null;
    }

    // Private function that processes a synchronous RetrieveGlobalOptionSetResponse
    function _retrieveGlobalOptionSetSyncParser(response, optionSetName) {
        var xmlDocument,
            optionSetMetadata,
            error;

        if (response.status === 200) { // Success
            xmlDocument = Sonoma.Xml.loadXml(response.responseText);

            optionSetMetadata = new GetOptionSetMetadata(xmlDocument.querySelector('ExecuteResult value'));

            Sonoma.Cache.set('Metadata.GlobalOptionSets', optionSetName, optionSetMetadata);

            response = null;
            return _getReturnObject(optionSetMetadata, true);
        }
        else {
            error = _getError(response);
            response = null;
            return _getReturnObject(error, false);
        }
    }

    // Private function that handles the RetrieveGlobalOptionSetRequest
    function _retrieveGlobalOptionSet(optionSetName, retrieveAsIfPublished, async) {
        var existingCache = Sonoma.Cache.get('Metadata.GlobalOptionSets', optionSetName),
            soap,
            parser;

        if (existingCache) {
            if (async) {
                return new Sonoma.Promise.Promise(function (resolve) {
                    resolve(existingCache);
                });
            }
            else {
                return _getReturnObject(existingCache, true);
            }
        }

        soap = _getRetrieveGlobalOptionSetSOAP(optionSetName, retrieveAsIfPublished);
        parser = async ? _retrieveGlobalOptionSetParser : _retrieveGlobalOptionSetSyncParser;

        return _sendRequest(soap, parser, async, optionSetName);
    }

    /**
        Sends an asynchronous RetrieveGlobalOptionSetRequest
        @method retrieveGlobalOptionSet
        @param optionSetName {string} Logical name of the global OptionSet.
        @param retrieveAsIfPublished {bool} Flag to retrieve only the OptionSet's published metadata.
        @return {Promise} Promise to return the OptionSet metadata
        @example
            Sonoma.Metadata.retrieveGlobalOptionSet('new_globaloptionset', true)
                .then(function success(optionSet) {
                    alert(optionSet.Options.length); // Number of options in the OptionSet
                },
                function failure(error) {
                    alert('Error: ' + error);
                });
            // → 5
    **/
    function retrieveGlobalOptionSet(optionSetName, retrieveAsIfPublished) {
        return _retrieveGlobalOptionSet(optionSetName, retrieveAsIfPublished, true);
    }


    /**
        Sends an synchronous RetrieveGlobalOptionSetRequest
        @method retrieveGlobalOptionSetSync
        @param optionSetName {string} Logical name of the global OptionSet.
        @param retrieveAsIfPublished {bool} Flag to retrieve only the OptionSet's published metadata.
        @return {Object} Result object
        @example
            var metadata,
                result = Sonoma.Metadata.retrieveGlobalOptionSetSync(currentModule.optionSetLogicalName, true);

            if (result.Success) {
                metadata = result.Value;
            }
    **/
    function retrieveGlobalOptionSetSync(optionSetName, retrieveAsIfPublished) {
        return _retrieveGlobalOptionSet(optionSetName, retrieveAsIfPublished, false);
    }

    //#endregion
    //#region AttributeMetadata Parsing Functions

    // Private function that returns the BigIntAttributeMetadata class from an XML node
    function _getBigIntAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.MaxValue = _getChildNodeText(node, 'MaxValue');
        attributeMetadata.MinValue = _getChildNodeText(node, 'MinValue');

        return attributeMetadata;
    }

    // Private function that returns the BooleanAttributeMetadata class from an XML node
    function _getBooleanAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.DefaultValue = _getNullableBoolean(_getChildNode(node, 'DefaultValue'));
        attributeMetadata.OptionSet = _getBooleanOptionSet(_getChildNode(node, 'OptionSet'));

        return attributeMetadata;
    }

    // Private function that returns the DateTimeAttributeMetadata class from an XML node
    function _getDateTimeAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.Format = _getChildNodeText(node, 'Format');
        attributeMetadata.ImeMode = _getChildNodeText(node, 'ImeMode');

        return attributeMetadata;
    }

    // Private function that returns the DecimalAttributeMetadata class from an XML node
    function _getDecimalAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.ImeMode = _getChildNodeText(node, 'ImeMode');
        attributeMetadata.MaxValue = _getChildNodeText(node, 'MaxValue');
        attributeMetadata.MinValue = _getChildNodeText(node, 'MinValue');
        attributeMetadata.Precision = _getNullableInt(_getChildNode(node, 'Precision'));

        return attributeMetadata;
    }

    // Private function that returns the DoubleAttributeMetadata class from an XML node
    function _getDoubleAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.ImeMode = _getChildNodeText(node, 'ImeMode');
        attributeMetadata.MaxValue = _getChildNodeText(node, 'MaxValue');
        attributeMetadata.MinValue = _getChildNodeText(node, 'MinValue');
        attributeMetadata.Precision = _getNullableInt(_getChildNode(node, 'Precision'));

        return attributeMetadata;
    }

    // Private function that returns the EntityNameAttributeMetadata class from an XML node
    function _getEntityNameAttributeMetadata(node) {
        return _createEnumAttributeMetadata(node);
    }

    // Private function that returns the IntegerAttributeMetadata class from an XML node
    function _getIntegerAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.Format = _getChildNodeText(node, 'Format');
        attributeMetadata.MaxValue = _getNullableInt(_getChildNode(node, 'MaxValue'));
        attributeMetadata.MinValue = _getNullableInt(_getChildNode(node, 'MinValue'));

        return attributeMetadata;
    }

    // Private function that returns the LookupAttributeMetadata class from an XML node
    function _getLookupAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node),
            targetNodes,
            i, len,
            targetNode;
        attributeMetadata.Targets = [];
        targetNodes = _getChildNode(node, 'Targets');
        if (targetNodes && targetNodes.childNodes) {
            for (i = 0, len = targetNodes.childNodes.length; i < len; i++) {
                targetNode = targetNodes.childNodes[i];
                if (!targetNode.text && !targetNode.textContent) {
                    continue;
                }
                attributeMetadata.Targets.push(targetNode.text || targetNode.textContent);
            }
        }
        return attributeMetadata;
    }

    // Private function that returns the ManagedPropertyAttributeMetadata class from an XML node
    function _getManagedPropertyAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.ManagedPropertyLogicalName = _getChildNodeText(node, 'ManagedPropertyLogicalName');
        attributeMetadata.ParentAttributeName = _getChildNodeText(node, 'ParentAttributeName');
        attributeMetadata.ParentComponentType = _getNullableInt(_getChildNode(node, 'ParentComponentType'));
        attributeMetadata.ValueAttributeTypeCode = _getChildNodeText(node, 'ValueAttributeTypeCode');

        return attributeMetadata;
    }

    // Private function that returns the MemoAttributeMetadata class from an XML node
    function _getMemoAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.Format = _getChildNodeText(node, 'Format');
        attributeMetadata.ImeMode = _getChildNodeText(node, 'ImeMode');
        attributeMetadata.MaxLength = _getNullableInt(_getChildNode(node, 'MaxLength'));

        return attributeMetadata;
    }

    // Private function that returns the MoneyAttributeMetadata class from an XML node
    function _getMoneyAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.CalculationOf = _getChildNodeText(node, 'CalculationOf');
        attributeMetadata.ImeMode = _getChildNodeText(node, 'ImeMode');
        attributeMetadata.MaxValue = _getChildNodeText(node, 'MaxValue');
        attributeMetadata.MinValue = _getChildNodeText(node, 'MinValue');
        attributeMetadata.Precision = _getNullableInt(_getChildNode(node, 'Precision'));
        attributeMetadata.PrecisionSource = _getNullableInt(_getChildNode(node, 'PrecisionSource'));

        return attributeMetadata;
    }

    // Private function that returns an enumerable AttributeMetadata class from an XML node
    function _createEnumAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.DefaultFormValue = _getNullableBoolean(_getChildNode(node, 'DefaultFormValue'));
        attributeMetadata.OptionSet = new GetOptionSetMetadata(_getChildNode(node, 'OptionSet'));

        return attributeMetadata;
    }

    // Private function that returns the PicklistAttributeMetadata class from an XML node
    function _getPicklistAttributeMetadata(node) {
        return _createEnumAttributeMetadata(node);
    }

    // Private function that returns the StateAttributeMetadata class from an XML node
    function _getStateAttributeMetadata(node) {
        return _createEnumAttributeMetadata(node);
    }

    // Private function that returns the StatusAttributeMetadata class from an XML node
    function _getStatusAttributeMetadata(node) {
        return _createEnumAttributeMetadata(node);
    }

    // Private function that returns the StringAttributeMetadata class from an XML node
    function _getStringAttributeMetadata(node) {
        var attributeMetadata = _getBaseAttributeMetadata(node);
        attributeMetadata.Format = _getChildNodeText(node, 'Format');
        attributeMetadata.ImeMode = _getChildNodeText(node, 'ImeMode');
        attributeMetadata.MaxLength = _getNullableInt(_getChildNode(node, 'MaxLength'));
        attributeMetadata.YomiOf = _getChildNodeText(node, 'YomiOf');

        return attributeMetadata;
    }

    // Private function that returns the base AttributeMetadata class from an XML node
    function _getBaseAttributeMetadata(node) {
        return {
            AttributeOf: _getChildNodeText(node, 'AttributeOf'),
            AttributeType: _getChildNodeText(node, 'AttributeType'),
            CanBeSecuredForCreate: _getNullableBoolean(_getChildNode(node, 'CanBeSecuredForCreate')),
            CanBeSecuredForRead: _getNullableBoolean(_getChildNode(node, 'CanBeSecuredForRead')),
            CanBeSecuredForUpdate: _getNullableBoolean(_getChildNode(node, 'CanBeSecuredForUpdate')),
            CanModifyAdditionalSettings: _getBooleanManagedProperty(_getChildNode(node, 'CanModifyAdditionalSettings')),
            ColumnNumber: _getNullableInt(_getChildNode(node, 'ColumnNumber')),
            DeprecatedVersion: _getChildNodeText(node, 'DeprecatedVersion'),
            Description: _getLabelSet(_getChildNode(node, 'Description')),
            DisplayName: _getLabelSet(_getChildNode(node, 'DisplayName')),
            EntityLogicalName: _getChildNodeText(node, 'EntityLogicalName'),
            ExtensionData: null,
            IsAuditEnabled: _getBooleanManagedProperty(_getChildNode(node, 'IsAuditEnabled')),
            IsCustomAttribute: _getNullableBoolean(_getChildNode(node, 'IsCustomAttribute')),
            IsCustomizable: _getBooleanManagedProperty(_getChildNode(node, 'IsCustomizable')),
            IsManaged: _getNullableBoolean(_getChildNode(node, 'IsManaged')),
            IsPrimaryId: _getNullableBoolean(_getChildNode(node, 'IsPrimaryId')),
            IsPrimaryName: _getNullableBoolean(_getChildNode(node, 'IsPrimaryName')),
            IsRenameable: _getBooleanManagedProperty(_getChildNode(node, 'IsRenameable')),
            IsSecured: _getNullableBoolean(_getChildNode(node, 'IsSecured')),
            IsValidForAdvancedFind: _getBooleanManagedProperty(_getChildNode(node, 'IsValidForAdvancedFind')),
            IsValidForCreate: _getNullableBoolean(_getChildNode(node, 'IsValidForCreate')),
            IsValidForRead: _getNullableBoolean(_getChildNode(node, 'IsValidForRead')),
            IsValidForUpdate: _getNullableBoolean(_getChildNode(node, 'IsValidForUpdate')),
            LinkedAttributeId: _getChildNodeText(node, 'LinkedAttributeId'),
            LogicalName: _getChildNodeText(node, 'LogicalName'),
            MetadataId: _getChildNodeText(node, 'MetadataId'),
            RequiredLevel: _getRequiredLevelManagedProperty(_getChildNode(node, 'RequiredLevel')),
            SchemaName: _getChildNodeText(node, 'SchemaName'),
            toString: function () { return this.LogicalName; }
        };
    }

    // Private function that creates an AttributeMetadata object from an XML node based on it's type
    function GetAttributeMetadata(node) {
        var attributeType = _getChildNode(node, 'AttributeType').textContent;

        switch (attributeType) {
            case 'BigInt':
                return _getBigIntAttributeMetadata(node);
            case 'Boolean':
                return _getBooleanAttributeMetadata(node);
            case 'CalendarRules':
                return _getLookupAttributeMetadata(node);
            case 'Customer':
                return _getLookupAttributeMetadata(node);
            case 'DateTime':
                return _getDateTimeAttributeMetadata(node);
            case 'Decimal':
                return _getDecimalAttributeMetadata(node);
            case 'Double':
                return _getDoubleAttributeMetadata(node);
            case 'EntityName':
                return _getEntityNameAttributeMetadata(node);
            case 'Integer':
                return _getIntegerAttributeMetadata(node);
            case 'Lookup':
                return _getLookupAttributeMetadata(node);
            case 'ManagedProperty':
                return _getManagedPropertyAttributeMetadata(node);
            case 'Memo':
                return _getMemoAttributeMetadata(node);
            case 'Money':
                return _getMoneyAttributeMetadata(node);
            case 'Owner':
                return _getLookupAttributeMetadata(node);
            case 'PartyList':
                return _getLookupAttributeMetadata(node);
            case 'Picklist':
                return _getPicklistAttributeMetadata(node);
            case 'State':
                return _getStateAttributeMetadata(node);
            case 'Status':
                return _getStatusAttributeMetadata(node);
            case 'String':
                return _getStringAttributeMetadata(node);
            case 'Uniqueidentifier':
                return _getBaseAttributeMetadata(node);
            case 'Virtual':
                return _getBaseAttributeMetadata(node);
            default:
                return _getBaseAttributeMetadata(node);
        }
    }

    //#endregion
    //#region Meta Metadata Functions

    /**
        Get the logical name of an entity given its type code.
        @method getEntityByTypeCode
        @param typeCode {int} Type code of the entity
        @return {Promise} Promise to return logical name of the entity
        @example
            Sonoma.Metadata.getEntityByTypeCode(4201)
                .then(function success(logicalName) {
                    alert(logicalName);
                },
                function failure(error) {
                    alert('Error: ' + error.message);
                });
            // → 'appointment'
    **/
    function getEntityByTypeCode(typeCode) {
        return retrieveAllEntities('Entity', false)
            .then(function (entityMetadataCollection) {
                var i,
                    error;

                for (i in entityMetadataCollection) {
                    if (entityMetadataCollection[i].ObjectTypeCode === typeCode) {
                        return entityMetadataCollection[i].LogicalName;
                    }
                }

                throw new Error('The entity with type code "' + typeCode + '" does not exist.');
            });
    }

    /**
        Get the type code of an entity given its logical name.
        @method getTypeCodeByEntity
        @param entityName {string} Logical Name of the entity
        @return {Promise} Promise to return the type code of the entity
        @example
            Sonoma.Metadata.getTypeCodeByEntity('appointment')
                .then(function success(typeCode) {
                    alert(typeCode);
                },
                function failure(error) {
                    alert('Error: ' + error.message);
                });
            // → 4201
    **/
    function getTypeCodeByEntity(entityName) {
        return retrieveEntity('Entity', entityName, false)
            .then(function (entityMetadata) {
                return entityMetadata.ObjectTypeCode;
            });
    }

    /**
        Synchronously return the type code of an entity given its logical name.
        @method getTypeCodeByEntitySync
        @param entityName {string} Logical Name of the entity
        @return {int} Type code of the entity
        @example
            var result = Sonoma.Metadata.getTypeCodeByEntity('appointment');

            if(result.Success) {
                alert(result.Value);
            }
            // → 4201
    **/
    function getTypeCodeByEntitySync(entityName) {
        var result = retrieveEntitySync(Sonoma.Metadata.entityFilters.Entity, entityName, false);

        if (result && result.Success === true) {
            return _getReturnObject(result.Value.ObjectTypeCode, true);
        }
        else {
            return result;
        }
    }

    /**
        Gets the logical name for the primary key attribute of an entity.
        @method getPrimaryKeyAttribute
        @param entityName {string} Logical name of the entity
        @return {Promise} Promise to receive logical name of the primary key
        @example
            Sonoma.Metadata.getPrimaryKeyAttribute('account')
                .then(function success(primaryKeyName) {
                    alert(primaryKeyName);
                },
                function failure(error) {
                    alert('Error: ' + error.message);
                });
            // → 'accountid'
    **/
    function getPrimaryKeyAttribute(entityName) {
        return retrieveEntity('Entity', entityName, false)
            .then(function (entityMetadata) {
                return entityMetadata.PrimaryNameAttribute;
            });
    }

    /**
        Synchronously return the logical name for the primary key attribute of an entity.
        @method getPrimaryKeyAttributeSync
        @param entityName {string} Logical Name of the entity
        @return {Object} Result object
        @example
            var result = Sonoma.Metadata.getPrimaryKeyAttribute('account');

            if (result.Success) {
                alert(result.Value);
            }
            // → 'accountid'
    **/
    function getPrimaryKeyAttributeSync(entityName) {
        var result = retrieveEntitySync(Sonoma.Metadata.entityFilters.Entity, entityName, false);

        if (result && result.Success === true) {
            return _getReturnObject(result.Value.PrimaryNameAttribute, true);
        }
        else {
            return result;
        }
    }

    /**
        Gets the integer value of an OptionSet option.
        @method getOptionSetValue
        @param entityName {string} Logical name of the entity
        @param attributeName {string} Logical name of the attribute
        @param optionName {string} Label for the option
        @return {Promise} Promise to return the option's integer value
        @example
            Sonoma.Metadata.getOptionSetValue('opportunity', 'opportunityratingcode', 'Cold')
                .then(function success(optionValue) {
                    alert(optionValue);
                },
                function failure(error) {
                    alert('Error: ' + error.message)
                });
            // → 3
    **/
    function getOptionSetValue(entityName, attributeName, optionName) {
        return retrieveAttribute(entityName, attributeName, false)
            .then(function (attributeMetadata) {
                var i = 0,
                    len,
                    error;

                len = attributeMetadata.OptionSet.Options.length;
                for (i = 0; i < len; i++) {
                    if (attributeMetadata.OptionSet.Options[i].Label.UserLocalizedLabel.Label === optionName) {
                        return attributeMetadata.OptionSet.Options[i].Value;
                    }
                }

                throw new Error('The option with name "' + optionName + '" does not exist.');
            });
    }

    /**
        Gets the integer value of an OptionSet option synchronously.
        @method getOptionSetValueSync
        @param entityName {string} Logical name of the entity
        @param attributeName {string} Logical name of the attribute
        @param optionName {string} Label for the option
        @return {Object} Result object
        @example
            var result = Sonoma.Metadata.getOptionSetValueSync('opportunity', 'opportunityratingcode', 'Cold');

            if (result.Success) {
                alert(result.Value);
            }
            // → 3
    **/
    function getOptionSetValueSync(entityName, attributeName, optionName) {
        var result = retrieveAttributeSync(entityName, attributeName, false),
            nameFound = false,
            i = 0, len,
            error,
            attributeMetadata;

        if (result && result.Success === true) {
            attributeMetadata = result.Value;

            len = attributeMetadata.OptionSet.Options.length;
            for (i = 0; i < len; i++) {
                if (attributeMetadata.OptionSet.Options[i].Label.UserLocalizedLabel.Label === optionName) {
                    return _getReturnObject(attributeMetadata.OptionSet.Options[i].Value, true);
                }
            }
            if (!nameFound) {
                error = new Error('The option with name "' + optionName + '" does not exist.');

                return _getReturnObject(error, false);
            }
        }
        else {
            return result;
        }
    }

    /**
        Gets the label for the integer value of an OptionSet option.
        @method getOptionSetOption
        @param entityName {string} Logical name of the entity
        @param attributeName {string} Logical name of the attribute
        @param optionName {int} Value of the option
        @return {Promise} Promise to return the option's label
        @example
            Sonoma.Metadata.getOptionSetOption('opportunity', 'opportunityratingcode', 2)
                .then(function success(optionName) {
                    alert(optionName);
                },
                function failure(error) {
                    alert('Error: ' + error.message)
                });
            // → 'Warm'
    **/
    function getOptionSetOption(entityName, attributeName, optionValue) {
        return retrieveAttribute(entityName, attributeName, false)
            .then(function (attributeMetadata) {
                var i = 0, len,
                    error;

                len = attributeMetadata.OptionSet.Options.length;
                for (i = 0; i < len; i++) {
                    if (attributeMetadata.OptionSet.Options[i].Value === optionValue) {
                        return attributeMetadata.OptionSet.Options[i].Label.UserLocalizedLabel.Label;
                    }
                }

                throw new Error('The option with value "' + optionValue + '" does not exist.');
            });
    }

    /**
        Returns the label for the integer value of an OptionSet option.
        @method getOptionSetOptionSync
        @param entityName {string} Logical name of the entity
        @param attributeName {string} Logical name of the attribute
        @param optionName {int} Value of the option
        @return {Object} Result object
        @example
            var result = Sonoma.Metadata.getOptionSetOptionSync('opportunity', 'opportunityratingcode', 2);

            if (result.Success) {
                alert(result.Value);
            }
            // → 'Warm'
    **/
    function getOptionSetOptionSync(entityName, attributeName, optionValue) {
        var result = retrieveAttributeSync(entityName, attributeName, false),
            valueFound = false,
            attributeMetadata,
            i = 0, len,
            error;

        if (result && result.Success === true) {
            attributeMetadata = result.Value;

            len = attributeMetadata.OptionSet.Options.length;
            for (i = 0; i < len; i++) {
                if (attributeMetadata.OptionSet.Options[i].Value === optionValue) {
                    return _getReturnObject(attributeMetadata.OptionSet.Options[i].Label.UserLocalizedLabel.Label, true);
                }
            }
            if (!valueFound) {
                error = new Error('The option with value "' + optionValue + '" does not exist.');

                return _getReturnObject(error, false);
            }
        }
        else {
            return result;
        }
    }

    //#endregion
    //#region All EntityMetadata Retrieval Functions

    // Private function that processes a RetrieveEntityResponse
    function _retrieveEntityParser(response, filterEnum, successCallback, errorCallback) {
        var entityMetadata,
            xmlDocument;
        if (response.readyState === 4) { // Complete
            if (response.status === 200) { // Success
                xmlDocument = Sonoma.Xml.loadXml(response.responseText);
                entityMetadata = _getEntityMetadata(xmlDocument.querySelector('ExecuteResult value'));
                _cacheEntityMetadata(entityMetadata, filterEnum);
                successCallback(entityMetadata);
            }
            else {
                errorCallback(_getError(response));
            }
        }
        response = null;
    }

    // Private function that processes a synchronous RetrieveEntityResponse
    function _retrieveEntitySyncParser(response, filterEnum) {
        var entityMetadata,
            xmlDocument,
            error;

        if (response.status === 200) { // Success
            xmlDocument = Sonoma.Xml.loadXml(response.responseText);
            entityMetadata = _getEntityMetadata(xmlDocument.querySelector('ExecuteResult value'));
            _cacheEntityMetadata(entityMetadata, filterEnum);
            response = null;
            return _getReturnObject(entityMetadata, true);
        }
        else {
            error = _getError(response);
            response = null;
            return _getReturnObject(error, false);
        }
    }

    // Private function that processes a RetrieveAllEntitiesResponse
    function _retrieveAllEntitiesParser(response, filterEnum, successCallback, errorCallback) {
        if (response.readyState === 4) { // Complete
            if (response.status === 200) { // Success
                var entityMetadataNodes,
                    xmlDocument,
                    entityMetadataCollection = {},
                    existingFilterLevel = 0,
                    entityList = [],
                    entityMetadata,
                    filterLevel,
                    i, len;

                xmlDocument = Sonoma.Xml.loadXml(response.responseText);
                entityMetadataNodes = xmlDocument.querySelectorAll('EntityMetadata');

                len = entityMetadataNodes.length;
                for (i = 0; i < len; i++) {
                    entityMetadata = _getEntityMetadata(entityMetadataNodes[i]);
                    entityMetadataCollection[entityMetadata.LogicalName] = entityMetadata;

                    if (i === 0) { existingFilterLevel = _cacheEntityMetadata(entityMetadata, filterEnum); }
                    /*jshint bitwise: false*/
                    else { existingFilterLevel = _cacheEntityMetadata(entityMetadata, filterEnum) & existingFilterLevel; }
                    /*jshint bitwise: true*/

                    entityList.push(entityMetadata.LogicalName);
                }

                // Determine what level of filter detail now exists for all entities
                /*jshint bitwise: false*/
                filterLevel = filterEnum | existingFilterLevel;
                /*jshint bitwise: true*/
                Sonoma.Cache.set('Metadata.AllEntities', 'FilterLevel', filterLevel);
                Sonoma.Cache.set('Metadata.AllEntities', 'EntityList', entityList);

                successCallback(entityMetadataCollection);
            }
            else {
                errorCallback(_getError(response));
            }
        }
        response = null;
    }

    /**
        Public function that sends an asynchronous RetrieveAllEntitiesRequest
        @method retrieveAllEntities
        @param entityFilters {int} One of the entity filters defined on the Sonoma.Metadata.entityFilters enum.
        @param retrieveAsIfPublished {bool} Flag to retrieve only the entity's published metadata.
        @returns {Promise} Promise to retrieve metadata for all entities
        @example
            var filter = Sonoma.Metadata.entityFilters['Attributes'];
            Sonoma.Metadata.retrieveAllEntities(filter, false)
                .then(function success(result) {
                    //
                },
                function failure(error) {
                    alert('Error: ' + error.message);
                });
    **/
    function retrieveAllEntities(entityFilters, retrieveAsIfPublished) {
        var filterEnum,
            existingFilterLevel,
            entityList,
            metadataCollection,
            i, len,
            willHaveFilterLevel,
            needFilterLevel,
            soap;

        filterEnum = _entityFilterStringToEnum(entityFilters);

        existingFilterLevel = Sonoma.Cache.get('Metadata.AllEntities', 'FilterLevel');
        entityList = Sonoma.Cache.get('Metadata.AllEntities', 'EntityList');
        /*jshint bitwise: false*/
        if (existingFilterLevel && entityList && ((existingFilterLevel & filterEnum) === filterEnum)) {
            metadataCollection = {};
            len = entityList.length;
            for (i = 0; i < len; i++) {
                metadataCollection[entityList[i]] = Sonoma.Cache.get('Metadata.Entities', entityList[i]).Metadata;
            }

            return new Sonoma.Promise.Promise(function (resolve) {
                resolve(metadataCollection);
            });
        }
        /*jshint bitwise: true*/

        willHaveFilterLevel = filterEnum;
        /*jshint bitwise: false*/
        if (existingFilterLevel) { willHaveFilterLevel = filterEnum | existingFilterLevel; }
        /*jshint bitwise: true*/

        needFilterLevel = filterEnum;
        /*jshint bitwise: false*/
        if (existingFilterLevel) { needFilterLevel = willHaveFilterLevel ^ existingFilterLevel; }
        /*jshint bitwise: true*/

        soap = [
            '<Execute xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services" ',
                'xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
              '<request i:type="a:RetrieveAllEntitiesRequest" ',
                  'xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts">',
                '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>EntityFilters</b:key>',
                    '<b:value i:type="c:EntityFilters" xmlns:c="http://schemas.microsoft.com/xrm/2011/Metadata">',
                      _getEntityFilterString(needFilterLevel),
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>RetrieveAsIfPublished</b:key>',
                    '<b:value i:type="c:boolean" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      !!retrieveAsIfPublished,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                '</a:Parameters>',
                '<a:RequestId i:nil="true" />',
                '<a:RequestName>RetrieveAllEntities</a:RequestName>',
              '</request>',
            '</Execute>'
        ];

        soap = _getSOAPWrapper(soap.join(''));

        return _sendRequest(soap, _retrieveAllEntitiesParser, true, filterEnum);
    }

    //#endregion
    //#region EntityMetadata Retrieval Functions

    // Private function that creates a SOAP RetrieveEntityRequest
    function _getRetrieveEntitySOAP(entityFilters, logicalName, retrieveAsIfPublished) {
        var soap = [
            '<Execute xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services" ',
                'xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
              '<request i:type="a:RetrieveEntityRequest" ',
                  'xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts">',
                '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>EntityFilters</b:key>',
                    '<b:value i:type="c:EntityFilters" xmlns:c="http://schemas.microsoft.com/xrm/2011/Metadata">',
                      _getEntityFilterString(entityFilters),
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>MetadataId</b:key>',
                    '<b:value i:type="ser:guid" xmlns:ser="http://schemas.microsoft.com/2003/10/Serialization/">',
                      '00000000-0000-0000-0000-000000000000',
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>RetrieveAsIfPublished</b:key>',
                    '<b:value i:type="c:boolean" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      !!retrieveAsIfPublished,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>LogicalName</b:key>',
                    '<b:value i:type="c:string" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      logicalName,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                '</a:Parameters>',
                '<a:RequestId i:nil="true" />',
                '<a:RequestName>RetrieveEntity</a:RequestName>',
              '</request>',
            '</Execute>'
        ];

        return _getSOAPWrapper(soap.join(''));
    }

    // Private function that handles the RetrieveEntityRequest
    function _retrieveEntity(entityFilters, logicalName, retrieveAsIfPublished, async) {
        var filterEnum = _entityFilterStringToEnum(entityFilters),
            existingCache = Sonoma.Cache.get('Metadata.Entities', logicalName),
            willHaveFilterLevel,
            needFilterLevel,
            soap,
            parser;

        /*jshint bitwise: false*/
        if (existingCache && ((existingCache.FilterLevel & filterEnum) === filterEnum)) {
            if (async) {
                return new Sonoma.Promise.Promise(function (resolve) {
                    resolve(existingCache.Metadata);
                });
            }
            else {
                return _getReturnObject(existingCache.Metadata, true);
            }
        }
        /*jshint bitwise: true*/

        willHaveFilterLevel = filterEnum;
        /*jshint bitwise: false*/
        if (existingCache) { willHaveFilterLevel = filterEnum | existingCache.FilterLevel; }
        /*jshint bitwise: true*/

        needFilterLevel = filterEnum;
        /*jshint bitwise: false*/
        if (existingCache) { needFilterLevel = willHaveFilterLevel ^ existingCache.FilterLevel; }
        /*jshint bitwise: true*/

        soap = _getRetrieveEntitySOAP(needFilterLevel, logicalName, retrieveAsIfPublished);
        parser = async ? _retrieveEntityParser : _retrieveEntitySyncParser;

        return _sendRequest(soap, parser, async, needFilterLevel);
    }

    /**
        Sends an asynchronous RetrieveEntityRequest
        @method retrieveEntity
        @param entityFilters {int} One of the entity filters defined on the Sonoma.Metadata.entityFilters enum.
        @param logicalName {string} Logical name of the entity
        @param retrieveAsIfPublished {bool} Flag to retrieve only the entity's published metadata.
        @return {Promise} Promise to return the entity metadata
        @example
            Sonoma.Metadata.retrieveEntity(Sonoma.Metadata.entityFilters.All, 'contact', true)
                .then(function success(entity) {
                    //
                },
                function failure(error) {
                    alert('Error: ' + error);
                });
    **/
    function retrieveEntity(entityFilters, logicalName, retrieveAsIfPublished) {
        return _retrieveEntity(entityFilters, logicalName, retrieveAsIfPublished, true);
    }

    /**
        Sends an synchronous RetrieveEntityRequest
        @method retrieveEntitySync
        @param entityFilters {int} One of the entity filters defined on the Sonoma.Metadata.entityFilters enum.
        @param logicalName {string} Logical name of the entity
        @param retrieveAsIfPublished {bool} Flag to retrieve only the entity's published metadata.
        @return {Object} Result object
        @example
            var metadata,
                result = Sonoma.Metadata.retrieveEntitySync(Sonoma.Metadata.entityFilters.All, currentModule.entityLogicalName, true);

            if (result.Success) {
                metadata = result.Value;
            }
    **/
    function retrieveEntitySync(entityFilters, logicalName, retrieveAsIfPublished) {
        return _retrieveEntity(entityFilters, logicalName, retrieveAsIfPublished, false);
    }

    //#endregion
    //#region AttributeMetadata Retrieval Functions

    // Private function that creates a SOAP RetrieveAttributeRequest
    function _getRetrieveAttributeSOAP(entityLogicalName, logicalName, retrieveAsIfPublished) {
        var soap = [
            '<Execute xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services" ',
                'xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
              '<request i:type="a:RetrieveAttributeRequest" ',
                  'xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts">',
                '<a:Parameters xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>MetadataId</b:key>',
                    '<b:value i:type="ser:guid" xmlns:ser="http://schemas.microsoft.com/2003/10/Serialization/">',
                      '00000000-0000-0000-0000-000000000000',
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>LogicalName</b:key>',
                    '<b:value i:type="c:string" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      logicalName,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>EntityLogicalName</b:key>',
                    '<b:value i:type="c:string" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      entityLogicalName,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                  '<a:KeyValuePairOfstringanyType>',
                    '<b:key>RetrieveAsIfPublished</b:key>',
                    '<b:value i:type="c:boolean" xmlns:c="http://www.w3.org/2001/XMLSchema">',
                      !!retrieveAsIfPublished,
                    '</b:value>',
                  '</a:KeyValuePairOfstringanyType>',
                '</a:Parameters>',
                '<a:RequestId i:nil="true" />',
                '<a:RequestName>RetrieveAttribute</a:RequestName>',
              '</request>',
            '</Execute>'
        ];

        return _getSOAPWrapper(soap.join(''));
    }

    // Private function that processes a RetrieveAttributeResponse
    function _retrieveAttributeParser(response, attrCacheKey, successCallback, errorCallback) {
        var attributeMetadata,
            xmlDocument;
        if (response.readyState === 4) { // Complete
            if (response.status === 200) { // Success
                xmlDocument = Sonoma.Xml.loadXml(response.responseText);
                attributeMetadata = new GetAttributeMetadata(xmlDocument.querySelector('ExecuteResult value'));
                Sonoma.Cache.set('Metadata.Attributes', attrCacheKey, attributeMetadata);

                successCallback(attributeMetadata);
            }
            else {
                errorCallback(_getError(response));
            }
        }

        response = null;
    }

    // Private function that processes a synchronous RetrieveAttributeResponse
    function _retrieveAttributeSyncParser(response, attrCacheKey) {
        var attributeMetadata,
            xmlDocument,
            error;

        if (response.status === 200) { // Success
            xmlDocument = Sonoma.Xml.loadXml(response.responseText);
            attributeMetadata = new GetAttributeMetadata(xmlDocument.querySelector('ExecuteResult value'));
            Sonoma.Cache.set('Metadata.Attributes', attrCacheKey, attributeMetadata);
            response = null;
            return _getReturnObject(attributeMetadata, true);
        }
        else {
            error = _getError(response);
            response = null;
            return _getReturnObject(error, false);
        }
    }

    // Private function that handles the RetrieveAttributeRequest
    function _retrieveAttribute(entityLogicalName, logicalName, retrieveAsIfPublished, async) {
        var attrCacheKey = entityLogicalName + '-' + logicalName,
            existingCache = Sonoma.Cache.get('Metadata.Attributes', attrCacheKey),
            soap,
            parser;

        if (existingCache) {
            if (async) {
                return new Sonoma.Promise.Promise(function (resolve) {
                    resolve(existingCache);
                });
            }
            else {
                return _getReturnObject(existingCache, true);
            }
        }

        soap = _getRetrieveAttributeSOAP(entityLogicalName, logicalName, retrieveAsIfPublished);
        parser = async ? _retrieveAttributeParser : _retrieveAttributeSyncParser;

        return _sendRequest(soap, parser, async, attrCacheKey);
    }

    /**
        Retrieves metadata for an entity's attribute.
        @method retrieveAttribute
        @param entityLogicalName {string} Logical name of the entity
        @param logicalName {string} Logical name of the attribute
        @param retrieveAsIfPublished {Bool} Flag to retrieve only the published metadata of the attribute
        @return {Promise} Promise to retrieve the metadata for the attribute
        @example
            Sonoma.Metadata.retrieveAttribute('new_customentity', 'new_customattribute', true)
                .then(function success(metadata) {
                    alert(metadata.AttributeType);
                },
                function failure(error) {
                    alert('Error: ' + error.message);
                });
            // → 'Picklist'

    **/
    function retrieveAttribute(entityLogicalName, logicalName, retrieveAsIfPublished) {
        return _retrieveAttribute(entityLogicalName, logicalName, retrieveAsIfPublished, true);
    }

    /**
        Synchronously retrieves metadata for an entity's attribute.
        @method retrieveAttributeSync
        @param entityLogicalName {string} Logical name of the entity
        @param logicalName {string} Logical name of the attribute
        @param retrieveAsIfPublished {Bool} Flag to retrieve only the published metadata of the attribute
        @return {Object} Result object
        @example
            var metadata,
                result = Sonoma.Metadata.retrieveAttribute('new_customentity', 'new_customattribute', true)
            if(result.Success) {
                metadata = result.Value;
            }
    **/
    function retrieveAttributeSync(entityLogicalName, logicalName, retrieveAsIfPublished) {
        return _retrieveAttribute(entityLogicalName, logicalName, retrieveAsIfPublished, false);
    }

    //#endregion
    //#region Public Functions

    return {
        entityFilters: entityFilters,
        
        // Public methods
        getEntityByTypeCode: getEntityByTypeCode,
        getTypeCodeByEntity: getTypeCodeByEntity,
        getTypeCodeByEntitySync: getTypeCodeByEntitySync,
        getOptionSetOption: getOptionSetOption,
        getOptionSetOptionSync: getOptionSetOptionSync,
        getOptionSetValue: getOptionSetValue,
        getOptionSetValueSync: getOptionSetValueSync,
        getPrimaryKeyAttribute: getPrimaryKeyAttribute,
        getPrimaryKeyAttributeSync: getPrimaryKeyAttributeSync,
        retrieveAllEntities: retrieveAllEntities,
        retrieveAttribute: retrieveAttribute,
        retrieveAttributeSync: retrieveAttributeSync,
        retrieveEntity: retrieveEntity,
        retrieveEntitySync: retrieveEntitySync,
        retrieveGlobalOptionSet: retrieveGlobalOptionSet,
        retrieveGlobalOptionSetSync: retrieveGlobalOptionSetSync
    };

    //#endregion
}());

/// <reference path='sonoma.js' />
/*global Sonoma XMLHttpRequest Q */
/**
    WebAPI Module
    @module Sonoma
    @submodule WebAPI
**/
/**
    WebAPI Utilities
    @class WebAPI
    @namespace Sonoma
**/
Sonoma.WebAPI = (function () {
    'use strict';

    function _getApiVersion() {
        var cachedVersion = Sonoma.Cache.get('WebAPI', 'ApiVersion'),
            currentVersion,
            globalContext;
        if (cachedVersion) { return cachedVersion; }
        if (window.Xrm && Xrm.Page && Xrm.Page.context && Xrm.Page.context.getVersion) {
            currentVersion = Xrm.Page.context.getVersion();
            currentVersion = (currentVersion) ? currentVersion.split('.').slice(0, 2).join('.') : '8.0';
        }
        else if (window.GetGlobalContext) {
            globalContext = window.GetGlobalContext();
            if (globalContext.getVersion) {
                currentVersion = globalContext.getVersion();
                currentVersion = (currentVersion) ? currentVersion.split('.').slice(0, 2).join('.') : '8.0';
            }
        }
        Sonoma.Cache.set('WebAPI', 'ApiVersion', currentVersion || '8.0');
        return currentVersion;
    }

    function _getDefaultHeaders(headers) {
        var _defaultHeaders = {
            'Accept': 'application/json',
            'Content-Type': 'application/json; charset=utf-8',
            'OData-MaxVersion': '4.0',
            'OData-Version': '4.0'
        };
        if (!headers) {return _defaultHeaders;}
        for (var key in headers) {
            if (headers.hasOwnProperty(key) && _defaultHeaders[key] === undefined) {
                _defaultHeaders[key] = headers[key];
            }
        }
        return _defaultHeaders;
    }

    function _parseJSON(value) {
        if (!value || typeof value !== 'string') { return null; }
        try {
            return JSON.parse(value);
        } catch (e) {
            return undefined;
        }
    }

    function _parseXML(value) {
        if (!value || typeof value !== 'string') { return null; }
        try {
            return Sonoma.Xml.loadXml(value);
        } catch (e) {
            return undefined;
        }
    }

    function _parseErrorResponse(request) {
        var errorAsJson = _parseJSON(request.responseText);
        if (errorAsJson && errorAsJson.error && errorAsJson.error.message) {
            return new Error(errorAsJson.error.message);
        }

        var errorAsXml = _parseXML(request.responseText);
        if (errorAsXml && errorAsXml.childNodes && errorAsXml.childNodes.length) {
            return new Error(errorAsXml.childNodes[0].textContent);
        }

        return new Error(request.responseText);
    }

    function _parseCreateResponse(request, resolve, reject) {
        var odataEntityId = request.getResponseHeader('OData-EntityId'),
            matches;
        if (odataEntityId) {
            matches = /\(([^)]+)\)/.exec(odataEntityId);
            if (matches && matches.length > 1) {
                resolve(matches[1]);
            }
            else {
                resolve(odataEntityId);
            }
        } 
        else {
            reject(new Error("Unknown response; Missing 'OData-EntityId' header."));
        }
    }

    function _parseGetResponse(request, resolve, reject) {
        var response = _parseJSON(request.responseText);
        if (response) {
            resolve(response);
        }
        else {
            reject(_parseErrorResponse(request));
        }
    }

    function _parseNoContentResponse(request, resolve) {
        resolve(true);
    }

    function _parsePostResponse(request, resolve, reject) {
        return (request.status === 204) ?
            _parseNoContentResponse(request, resolve, reject) :
            _parseGetResponse(request, resolve, reject);
    }

    function _parseUpsertResponse(request, resolve, reject) {
        return (request.status === 204) ? 
            _parseNoContentResponse(request, resolve, reject) :
            _parseCreateResponse(request, resolve, reject);
    }

    function _handleResponse(request, parser, resolve, reject) {
        if (request.readyState === 4) {
            if (request.status === 200 || request.status === 204) {
                parser(request, resolve, reject);
            }
            else if (request.status === 0) {
                request = null;
                return;
            }
            else {
                reject(_parseErrorResponse(request));
            }
            request = null;
        }
    }

    function _executeRequest(options) {
        var deferred = Sonoma.Promise.defer(),
            request = new XMLHttpRequest(),
            headers = {},
            url;

        url = [Sonoma.getClientUrl(), '/api/data/v', _getApiVersion(), '/', options.path].join('');

        request.open(options.method, url, true);

        headers = _getDefaultHeaders(options.headers);
        for (var key in headers) {
            if (headers.hasOwnProperty(key)) {
                request.setRequestHeader(key, headers[key]);
            }
        }

        request.onreadystatechange = function onReadyStateChange() {
            _handleResponse(request, options.parser, deferred.resolve, deferred.reject);
        };
        request.send(options.data);

        return deferred.promise;
    }

    function _executeDELETE(path) {
        return _executeRequest({
            data: null,
            headers: {},
            method: 'DELETE',
            path: path,
            parser: _parseNoContentResponse
        });
    }

    function _executeGET(path, headers) {
        return _executeRequest({
            data: null,
            headers: headers,
            method: 'GET', 
            path: path,
            parser: _parseGetResponse
        });
    }

    function _executePATCH(path, headers, data) {
        return _executeRequest({
            data: data,
            headers: headers,
            method: 'PATCH', 
            path: path,
            parser: _parseUpsertResponse
        });
    }

    function _executePOST(path, headers, data, parser) {
        return _executeRequest({
            data: data,
            headers: headers,
            method: 'POST', 
            path: path,
            parser: parser
        });
    }

    /**
        Create a new entity in CRM.
        @method create
        @param entitySet {String} Set name of the entity
        @param entity {Object} The entity to be created
        @return {Promise} Promise to return the id of the created entity
        @example
            // Javascript object with CRM logical names as properties
            var newAccount = {
                name: 'New Account',
                accountnumber: '1234'
            };
    
            Sonoma.WebAPI.create('accounts', newAccount)
                .then(function (id) {
                    // 'id' is generated by CRM
                    alert(id);
                },
                function (error) {
                    alert(error);
                });
    
            // → 'f05edb45-dc67-4df4-ba48-8930a6532360'
    **/
    function create(entitySet, entity) {
        if (!entitySet) {
            return Sonoma.Promise.reject(new Error("'entitySet' is a required parameter"));
        }
        if (!entity) {
            return Sonoma.Promise.reject(new Error("'entity' is a required parameter"));
        }
        return _executePOST(entitySet, {}, JSON.stringify(entity), _parseCreateResponse);
    }

    /**
        Delete an entity in CRM.
        @method destroy
        @param entitySet {String} Set name of the entity
        @param id {String} Guid of the entity
        @param referencePath {String} Optional path of the targeted navigation property, e.g., disassociate
        @return {Promise}
        @example
            var leadId = 'cca4ceb5-6869-41b0-a632-2fea0634ecdf';
    
            Sonoma.WebAPI.destroy('leads', leadId)
                .then(function () {
                    // success
                },
                function (error) {
                    alert(error);
                });
    
            // → 'Record deleted'
    **/
    function destroy(entitySet, id, referencePath) {
        if (!entitySet) {
            return Sonoma.Promise.reject(new Error("'entitySet' is a required parameter"));
        }
        if (!id) {
            return Sonoma.Promise.reject(new Error("'id' is a required parameter"));
        }
        var path = [entitySet,'(', id, ')', referencePath || ''].join('');
        return _executeDELETE(path);
    }

    /**
        Retrive multiple records from an entity set using FetchXML
        @method fetch
        @param entitySet {String} Set name of the entity
        @param fetchXml {String} The FetchXML request
        @return {Promise} Promise to return the entities data
        @example
            var fetchXml = [
                '<fetch mapping="logical" count="100" version="1.0">',
                    '<entity name="systemuser">',
                        '<attribute name="fullname" />',
                    '</entity>',
                '</fetch>'
            ].join('');
    
            Sonoma.WebAPI.fetch('systemusers', fetchXml)
                .then(function(result) {
                    alert('Returned ' + result.value.length + ' results');
                },
                function(error) {
                    alert(error);
                });
    
            // → 'Returned 100 results'
    **/
    function fetch(entitySet, fetchXml) {
        if (!entitySet) {
            return Sonoma.Promise.reject(new Error("'entitySet' is a required parameter"));
        }
        if (!fetchXml) {
            return Sonoma.Promise.reject(new Error("'fetchXml' is a required parameter"));
        }
        var encodedFetchXml = encodeURIComponent(fetchXml),
            path = [entitySet, '?fetchXml=', encodedFetchXml].join('');

        return _executeGET(path, {'Prefer': 'odata.include-annotations="*"'});
    }

    /**
        Executes a get request for a given path and query
        @method get
        @param path {String} Path of the request
        @return {Promise} Promise to return the response data
        @example
            Sonoma.WebAPI.get("EntityDefinitions?$filter=SchemaName eq 'Account'")
                .then(function(res) {
                    alert(res.value);
                })
                .catch(function(err) {
                    Sonoma.Log.error(err);
                });
    **/
    function get(path) {
        if (!path) {
            return Sonoma.Promise.reject(new Error("'path' is a required parameter"));
        }
        return _executeGET(path, {});
    }

    /**
        Executes a post request for a given path and payload
        @method post
        @param path {String} Path of the request
        @param data {Object} Payload to be posted in the body of the request
        @return {Promise} Promise to return
        @example
            var data = {
                "Status": 3,
                "OpportunityClose": {
                  "subject": "Won Opportunity",
                  "opportunityid@odata.bind": "[Organization URI]/api/data/v8.1/opportunities(b3828ac8-917a-e511-80d2-00155d2a68d2)"
              }
            };
            Sonoma.WebAPI.post('WinOpportunity', data)
                .then(function() {
                    // success
                })
                .catch(function(err) {
                    Sonoma.Log.error(err);
                });
    **/
    function post(path, data) {
        if (!path) {
            return Sonoma.Promise.reject(new Error("'path' is a required parameter"));
        }
        if (!data) {
            return Sonoma.Promise.reject(new Error("'data' is a required parameter"));
        } 
        return _executePOST(path, {}, JSON.stringify(data), _parsePostResponse);
    }

    /**
        Query records from an entity set

        See https://msdn.microsoft.com/en-us/library/gg334767.aspx
        @method query
        @param entitySet {String} Set name of the entity
        @param params {String} System query options
        @return {Promise} Promise to return the entities data
        @example
            Sonoma.WebAPI.query('accounts', '$select=name&$top=10')
                .then(function(result) {
                    alert('Returned ' + result.value.length + ' results');
                },
                function(error) {
                    alert(error);
                });
    
            // → 'Returned 10 results'
    **/
    function query(entitySet, params) {
        if (!entitySet) {
            return Sonoma.Promise.reject(new Error("'entitySet' is a required parameter"));
        }
        var path = [entitySet, '?', params||''].join('');
        return _executeGET(path, {'Prefer': 'odata.include-annotations="*"'});
    }

    function queryAll(entitySet, params) {
        throw new Error('Not Implemented.');
    }

    /**
        Upsert an entity in CRM.
        @method upsert
        @param entitySet {String} Set name of the entity
        @param id {String} Guid of entity updated
        @param entity {Object} The entity to be updated
        @return {Object} Promise to return the id of the updated entity
        @example
            // Guid of the entity
            var updateAccountId = 'f05edb45-dc67-4df4-ba48-8930a6532360';
    
            // Javascript object with CRM properties to update
            var updateAccount = {
                name: 'Updated Account',
                description: 'I\'m afraid the deflector shield will be quite operational when your friends arrive.'
            };
    
            Sonoma.WebAPI.upsert('accounts', updateAccountId, updateAccount)
                .then(function () {
                    // success
                },
                function (error) {
                    alert(error);
                }
            );
    **/
    function upsert(entitySet, id, entity) {
        if (!entitySet) {
            return Sonoma.Promise.reject(new Error("'entitySet' is a required parameter"));
        }
        if (!id) {
            return Sonoma.Promise.reject(new Error("'id' is a required parameter"));
        }
        var path = typeof(id) === 'string' ? [entitySet,'(', id, ')'].join('') : entitySet;
        return _executePATCH(path, {}, JSON.stringify(entity));
    }

    return {
        create: create,
        destroy: destroy,
        fetch: fetch,
        get: get,
        query: query,
        queryAll: queryAll,
        post: post,
        upsert: upsert
    };
}());

/// <reference path="sonoma.js" />
/// <reference path="core.js" />
/// <reference path="array.js" />
/// <reference path="orgservice.js" />
/// <reference path="guid.js" />
/// <reference path="string.js" />
/// <reference path="cache.js" />
/*globals GetGlobalContext */
/**
    User Module
    @module Sonoma
    @submodule User
**/
/**
    Security role and team utilities to retrieve or comapare user data
    @class User
    @namespace Sonoma
**/
Sonoma.User = (function () {
    var _cacheTarget = 'Sonoma.User';

    //#region Role functions

    /**
        Check if the the current user has any security role in a provided list.
        @method hasRole
        @param ...roleString {string} Security role name(s)
        @return {Promise} Promise to return true/false if the user has any role 
        @example
            // Current user has 'Sonoma Marketing' role
            Sonoma.User.hasRole('Sonoma Salesperson', 'Sonoma Marketing')
                .done(function success(hasRole) {
                    alert(hasRole);
                },
                function failure(error) {
                    alert(error);
                });
            // → true
    **/
    function hasRole() {
        var desiredRoles = Array.prototype.concat.apply([], arguments);

        return _getRoleNamesInternal(false)
            .then(function (userRoles) {
                if (!userRoles) {
                    return undefined;
                }

                return _searchNames(desiredRoles, userRoles);
            });
    }

    /**
        Check if the the user has any security role in a provided list, synchronously.
        @method hasRoleSync
        @param ...string {string} Security role name(s)
        @return {boolean} True/False if the current user has any role 
        @example
            // Current user has 'Sonoma Marketing' role
            Sonoma.User.hasRoleSync('Sonoma Salesperson', 'Sonoma Marketing');
            // → true
    **/
    function hasRoleSync() {
        var desiredRoles = Array.prototype.concat.apply([], arguments),
            userRoles = _getRoleNamesInternal(true);

        if (!userRoles) {
            return false;
        }

        return _searchNames(desiredRoles, userRoles);
    }

    /**
        Get the names of the current user's security roles.
        @method getRoles
        @return {Promise} Promise to return array of security role names 
        @example
            Sonoma.User.getRoles()
                .done(function (roleNames) {
                    alert(roleNames.join(', '));
                },
                function (error) {
                    alert(error);
                });
            // → 'Sonoma Marketing, System Administrator'
    **/
    function getRoles() {
        return _getRoleNamesInternal(false);
    }

    /**
        Get the names of the current user's security roles, synchronously.
        @method getRolesSync
        @return {Array} Array of security role names
        @example
            Sonoma.User.getRolesSync()
            // → ['Sonoma Marketing', 'System Administrator']
    **/
    function getRolesSync() {
        return _getRoleNamesInternal(true);
    }

    function _getRoleNamesInternal(isSync) {
        /* jshint newcap: false */
        var roleIdFilter = '', uncachedRoles = [], requestedNames = [],
            fetch, result,
            roleIds = _getUserRoleIds();

        if (!roleIds) {
            return isSync ? [] : Sonoma.Promise.resolve([]);
        }

        Sonoma.each(roleIds, function (i, id) {
            if (!_getCachedValue('Roles', Sonoma.Guid.format(id))) {
                roleIdFilter += '<condition attribute="roleid" operator="eq" value="' + id + '" />';
            }
        });

        fetch = [
            '<fetch mapping="logical" version="1.0">',
                '<entity name="role">',
                    '<attribute name="name" />',
                    '<attribute name="roleid" />',
                    '<filter type="or">',
                        roleIdFilter,
                    '</filter>',
                '</entity>',
            '</fetch>'].join('');

        if (isSync) {
            result = Sonoma.OrgService.retrieveMultipleSync(fetch);

            Sonoma.each(result.Value.Entities, function (i, role) {
                _setCachedValue('Roles', Sonoma.Guid.format(role.roleid.Value), role.name);
            });

            Sonoma.each(roleIds, function (i, id) {
                requestedNames.push(_getCachedValue('Roles', Sonoma.Guid.format(id)));
            });

            return requestedNames;
        }
        else {
            return Sonoma.OrgService.retrieveMultiple(fetch)
                .then(function (result) {
                    Sonoma.each(result.Entities, function (i, role) {
                        _setCachedValue('Roles', Sonoma.Guid.format(role.roleid.Value), role.name);
                    });

                    Sonoma.each(roleIds, function (i, id) {
                        requestedNames.push(_getCachedValue('Roles', Sonoma.Guid.format(id)));
                    });

                    return requestedNames;
                });
        }
    }

    //#endregion

    //#region Helper functions

    // compare a user's team/role names against a desired set
    function _searchNames(desiredNames, userNames) {
        var hasRoles = false,
            i;

        for (i = 0; i < desiredNames.length; i++) {
            if (Sonoma.Array.indexOf(userNames, desiredNames[i]) > -1) {
                return true;
            }
        }

        return false;
    }

    function _getCachedValue(cacheName, key) {
        var valueTable = Sonoma.Cache.get(_cacheTarget, cacheName) || {};
        return valueTable[key] || null;
    }

    function _setCachedValue(cacheName, key, value) {
        var valueTable = Sonoma.Cache.get(_cacheTarget, cacheName) || {};
        valueTable[key] = value;
        Sonoma.Cache.set(_cacheTarget, cacheName, valueTable);
    }

    function _getUserRoleIds() {
        /* jshint newcap: false */
        var roleIds = null;

        if (window.Xrm && Xrm.Page && Xrm.Page.context) {
            roleIds = Xrm.Page.context.getUserRoles();
        }
        else if (window.GetGlobalContext) {
            roleIds = GetGlobalContext().getUserRoles();
        }

        if (!roleIds) {
            alert('Unable to determine the user\'s roles from Xrm.Page.context.  Please include ClientGlobalContext.js.aspx.');
            return;
        }

        return roleIds;
    }
    
    //#endregion

    //#region Team functions

    /**
        Get the id and name for each of the user's teams.
        @method getTeams
        @return {Promise} Promise to return array team data
        @example
            // User belongs to Sonoma Account Team and Microsoft Account Team
            Sonoma.User.getTeams()
                .done(function success(teams) {
                    Sonoma.Log.log(teams);
                },
                function failure(error) {
                    alert(error);
                });
            // → [
            //      { id: '65ee700e-6e1c-4245-b142-86727fc8862d', name: 'Sonoma Account Team' }, 
            //      { id: '5a890091-f7c6-4f6a-83cd-cd45eb10061e', name: 'Microsoft Account Team' }
            //  ]
    **/
    function getTeams() {
        return _getTeamsInternal(false);
    }

    /**
        Get the id and name for each of the user's teams, synchronously.
        @method getTeamsSync
        @return {Array} Array of team data
        @example
            // User belongs to Sonoma Account Team and Microsoft Account Team
            var teams = Sonoma.User.getTeamsSync();
    
            Sonoma.each(teams, function (i, team) {
                alert(team.name);
            });
    
            // → 'Sonoma Account Team' 
            // → 'Microsoft Account Team'
    **/
    function getTeamsSync() {
        return _getTeamsInternal(true);
    }

    /**
        Check if the user belongs to any team in a provided list.
        @method belongsToTeam
        @param ...teamNames {String} The teams to check
        @return {Promise} Promise to return true/false if user belongs to team
        @example
            // User belongs to Sonoma Account Team
            Sonoma.User.belongsToTeam('Sonoma Account Team', 'Golden Tee Account Team')
                .done(function (belongsToTeam) {
                    alert(belongsToTeam);
                },
                function (error) {
                    alert(error);
                });
            // → true
    **/
    function belongsToTeam() {
        var desiredTeams = Array.prototype.concat.apply([], arguments),
            teamNames;

        return _getTeamsInternal(false)
            .then(function (teams) {
                if (!teams) {
                    return false;
                }

                teamNames = [];
                Sonoma.each(teams, function (i, team) {
                    teamNames.push(team.name);
                });
                return _searchNames(desiredTeams, teamNames);
            });
    }

    /**
        Check if the user belongs to any team in a provided list, synchronously.
        @method belongsToTeamSync
        @param ...teamNames {String} The teams to check
        @return {boolean} True/False if user belongs to team
        @example
            // User belongs to Sonoma Account Team
            Sonoma.User.belongsToTeamSync('Sonoma Account Team', 'Golden Tee Account Team')
            // → true
    **/
    function belongsToTeamSync() {
        var desiredTeams = Array.prototype.concat.apply([], arguments),
            teams = _getTeamsInternal(true),
            teamNames = [];

        if (!teams) {
            return false;
        }

        Sonoma.each(teams, function (i, team) {
            teamNames.push(team.name);
        });
        return _searchNames(desiredTeams, teamNames);
    }

    function _getTeamsInternal(isSync) {
        /* jshint newcap: false */

        var query,
            result,
            teamsCache = Sonoma.Cache.get(_cacheTarget, 'Teams');

        if (isSync && teamsCache) {
            return teamsCache.slice(0);
        }
        else if (isSync) {
            teamsCache = [];
            query = _getTeamsFetch();
            result = Sonoma.OrgService.retrieveMultipleSync(query);

            if (!result.Success || !result.Value.Entities || result.Value.Entities.length === 0) {
                return [];
            }

            Sonoma.each(result.Value.Entities, function (i, team) {
                teamsCache.push({
                    name: team.name,
                    id: Sonoma.Guid.format(team.teamid.Value)
                });
            });

            Sonoma.Cache.set(_cacheTarget, 'Teams', teamsCache);
            return teamsCache.slice(0);
        }
        else if (teamsCache) {
            return Sonoma.Promise.resolve(teamsCache.slice(0));
        }
        else {
            teamsCache = [];
            query = _getTeamsFetch();
            return Sonoma.OrgService.retrieveMultiple(query)
                .then(function (result) {
                    if (!result.Entities || result.Entities.length === 0) {
                        return [];
                    }
                        
                    Sonoma.each(result.Entities, function (i, team) {
                        teamsCache.push({ 
                            name: team.name,
                            id: Sonoma.Guid.format(team.teamid.Value)
                        });
                    });

                    Sonoma.Cache.set(_cacheTarget, 'Teams', teamsCache);
                    return teamsCache.slice(0);
                });
        }
    }

    function _getTeamsFetch() {
        return [
            '<fetch mapping="logical" version="1.0">',
                '<entity name="team">',
                    '<attribute name="name" />',
                    '<attribute name="teamid" />',
                    '<link-entity name="teammembership" from="teamid" to="teamid">',
                        '<filter>',
                            '<condition attribute="systemuserid" operator="eq-userid" />',
                        '</filter>',
                    '</link-entity>',
                '</entity>',
            '</fetch>'
        ].join('');
    }

    //#endregion

    return {
        getRoles: getRoles,
        getRolesSync: getRolesSync,
        hasRole: hasRole,
        hasRoleSync: hasRoleSync,
        getTeams: getTeams,
        getTeamsSync: getTeamsSync,
        belongsToTeam: belongsToTeam,
        belongsToTeamSync: belongsToTeamSync
    };

}());

this.SonomaCmc = Sonoma;
}).call(this);