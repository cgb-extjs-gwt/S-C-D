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
import { FormPanel, NumberField, Button } from '@extjs/ext-react';
var EditCellsPanel = (function (_super) {
    __extends(EditCellsPanel, _super);
    function EditCellsPanel() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    EditCellsPanel.prototype.render = function () {
        var _this = this;
        var _a = this.props, value = _a.value, onEditClick = _a.onEditClick, onCancelClick = _a.onCancelClick;
        return (React.createElement(FormPanel, null,
            React.createElement(NumberField, { ref: function (field) { return _this.numberField = field; }, label: "Value:", value: value }),
            React.createElement(Button, { text: "Edit", handler: function () { return onEditClick(_this.numberField.getValue()); } }),
            React.createElement(Button, { text: "Cancel", handler: onCancelClick })));
    };
    return EditCellsPanel;
}(React.Component));
export default EditCellsPanel;
//# sourceMappingURL=EditCellsPanel.js.map