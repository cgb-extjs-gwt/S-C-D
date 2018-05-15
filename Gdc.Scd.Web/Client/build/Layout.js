var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
import * as React from 'react';
import { Container, TitleBar, Button, Sheet, Panel } from '@extjs/ext-react';
import { Switch, Route, withRouter } from 'react-router-dom';
import { medium, large } from './responsiveFormulas';
import Home from './Home/Home';
import About from './About/About';
import NavMenu from './NavMenu';
import { ScdPivotGrid } from './ScdPivotGrid';
var Layout = (function (_super) {
    __extends(Layout, _super);
    function Layout() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.state = {
            showAppMenu: false
        };
        _this.toggleAppMenu = function () {
            _this.setState({ showAppMenu: !_this.state.showAppMenu });
        };
        _this.onHideAppMenu = function () {
            _this.setState({ showAppMenu: false });
        };
        _this.navigate = function (path) {
            _this.setState({ showAppMenu: false });
            _this.props.history.push(path);
        };
        return _this;
    }
    Layout.prototype.render = function () {
        var showAppMenu = this.state.showAppMenu;
        var location = this.props.location;
        var navMenuDefaults = {
            onItemClick: this.navigate,
            selection: location.pathname
        };
        return (React.createElement(Container, { fullscreen: true, layout: "fit" },
            React.createElement(TitleBar, { title: "test", docked: "top" }, Ext.platformTags.phone && (React.createElement(Button, { align: "left", iconCls: "x-fa fa-bars", handler: this.toggleAppMenu, ripple: false }))),
            Ext.platformTags.phone ? (React.createElement(Sheet, { displayed: showAppMenu, side: "left", onHide: this.onHideAppMenu },
                React.createElement(Panel, { scrollable: true, title: "ExtReact Boilerplate" },
                    React.createElement(NavMenu, __assign({}, navMenuDefaults, { width: "250" }))))) : (React.createElement(Panel, { scrollable: true, docked: "left", shadow: true, zIndex: 2 },
                React.createElement(NavMenu, __assign({}, navMenuDefaults, { responsiveConfig: (_a = {},
                        _a[medium] = {
                            micro: true,
                            width: 56
                        },
                        _a[large] = {
                            micro: false,
                            width: 200
                        },
                        _a) })))),
            React.createElement(Switch, null,
                React.createElement(Route, { path: "/", component: Home, exact: true }),
                React.createElement(Route, { path: "/about", component: About }),
                React.createElement(Route, { path: "/pivot", component: ScdPivotGrid }))));
        var _a;
    };
    return Layout;
}(React.Component));
export default withRouter(Layout);
//# sourceMappingURL=Layout.js.map