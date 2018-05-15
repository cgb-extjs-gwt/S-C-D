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
import * as React from 'react';
import { Grid, Toolbar, Column, SearchField } from '@extjs/ext-react';
import data from './data';
import { small, medium } from '../responsiveFormulas';
var Home = (function (_super) {
    __extends(Home, _super);
    function Home() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.store = Ext.create('Ext.data.Store', {
            fields: ['name', 'email', 'phone', 'hoursTaken', 'hoursRemaining'],
            data: data
        });
        _this.onSearch = function () {
            var query = _this.query.getValue().toLowerCase();
            _this.store.clearFilter();
            if (query.length)
                _this.store.filterBy(function (record) {
                    var _a = record.data, name = _a.name, email = _a.email, phone = _a.phone;
                    return name.toLowerCase().indexOf(query) !== -1 ||
                        email.toLowerCase().indexOf(query) !== -1 ||
                        phone.toLowerCase().indexOf(query) !== -1;
                });
        };
        return _this;
    }
    Home.prototype.render = function () {
        var _this = this;
        return (React.createElement(Grid, { store: this.store },
            React.createElement(Toolbar, { docked: "top" },
                React.createElement(SearchField, { ui: "faded", ref: function (field) { return _this.query = field; }, placeholder: "Search...", onChange: this.onSearch.bind(this), responsiveConfig: (_a = {},
                        _a[small] = {
                            flex: 1
                        },
                        _a[medium] = {
                            flex: undefined
                        },
                        _a) })),
            React.createElement(Column, { text: "Name", dataIndex: "name", flex: 2, resizable: true }),
            React.createElement(Column, { text: "Email", dataIndex: "email", flex: 3, resizable: true, responsiveConfig: (_b = {},
                    _b[small] = {
                        hidden: true
                    },
                    _b[medium] = {
                        hidden: false
                    },
                    _b) }),
            React.createElement(Column, { text: "Phone", dataIndex: "phone", flex: 2, resizable: true })));
        var _a, _b;
    };
    return Home;
}(React.Component));
export default Home;
//# sourceMappingURL=Home.js.map