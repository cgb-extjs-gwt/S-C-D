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
import { Dialog } from '@extjs/ext-react';
import EditCellsPanel from './EditCellsPanel';
var EditCellsWindow = (function (_super) {
    __extends(EditCellsWindow, _super);
    function EditCellsWindow() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    EditCellsWindow.prototype.render = function () {
        var _this = this;
        var _a = this.props, isVisible = _a.isVisible, value = _a.value, onEditClick = _a.onEditClick, onCancelClick = _a.onCancelClick;
        return (React.createElement(Dialog, { displayed: isVisible, title: "Edit Cells", closeAction: "hide", ref: function (dialog) { return _this.dialog = dialog; } },
            React.createElement(EditCellsPanel, { value: value, onEditClick: onEditClick, onCancelClick: onCancelClick })));
    };
    return EditCellsWindow;
}(React.Component));
export default EditCellsWindow;
//# sourceMappingURL=EditCellsWindow.js.map