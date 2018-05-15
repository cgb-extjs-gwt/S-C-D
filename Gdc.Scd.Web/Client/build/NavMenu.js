var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
var __rest = (this && this.__rest) || function (s, e) {
    var t = {};
    for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p) && e.indexOf(p) < 0)
        t[p] = s[p];
    if (s != null && typeof Object.getOwnPropertySymbols === "function")
        for (var i = 0, p = Object.getOwnPropertySymbols(s); i < p.length; i++) if (e.indexOf(p[i]) < 0)
            t[p[i]] = s[p[i]];
    return t;
};
import * as React from 'react';
import { TreeList } from '@extjs/ext-react';
Ext.require('Ext.data.TreeStore');
var NavMenu = function (_a) {
    var onItemClick = _a.onItemClick, selection = _a.selection, props = __rest(_a, ["onItemClick", "selection"]);
    return (React.createElement(TreeList, __assign({}, props, { ui: "nav", expanderFirst: false, onItemClick: function (tree, item) { return onItemClick(item.node.getId()); }, selection: selection, store: {
            root: {
                children: [
                    { id: '/', text: 'Home', iconCls: 'x-fa fa-home', leaf: true },
                    { id: '/about', text: 'About', iconCls: 'x-fa fa-info', leaf: true },
                    { id: '/pivot', text: 'Pivot grid', iconCls: 'x-fa fa-info', leaf: true },
                ]
            }
        } })));
};
export default NavMenu;
//# sourceMappingURL=NavMenu.js.map