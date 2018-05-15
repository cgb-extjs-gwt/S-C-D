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
import { PivotGrid } from '@extjs/ext-react-pivot';
import { Toolbar, Button, Menu, MenuItem, Container } from '@extjs/ext-react';
import EditCellsWindow from './EditCellsWindow';
var AggregateValueInfo = (function () {
    function AggregateValueInfo() {
    }
    return AggregateValueInfo;
}());
var AggregateType;
(function (AggregateType) {
    AggregateType[AggregateType["WithoutAggregation"] = 0] = "WithoutAggregation";
    AggregateType[AggregateType["InstallBaseValue"] = 1] = "InstallBaseValue";
    AggregateType[AggregateType["InstallBaseAggregate"] = 2] = "InstallBaseAggregate";
})(AggregateType || (AggregateType = {}));
var ScdPivotGrid = (function (_super) {
    __extends(ScdPivotGrid, _super);
    function ScdPivotGrid() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.onEditCellDialogClick = function (value) {
            if (_this.selection.startCell && _this.selection.endCell) {
                _this.updateValues(_this.pivotgrid, _this.selectedRecords, _this.selection.startCell.columnIndex, _this.selection.endCell.columnIndex, value);
            }
            _this.setState(__assign({}, _this.state, { isVisibleEditDialog: false }));
        };
        _this.onCancelDialogClick = function () {
            _this.setState(__assign({}, _this.state, { isVisibleEditDialog: false }));
        };
        _this.onSelectionChange = function (view, records, selected, selection) {
            _this.selectedRecords = records;
            _this.selection = selection;
        };
        _this.onEditClick = function () {
            _this.setState(__assign({}, _this.state, { isVisibleEditDialog: true }));
        };
        return _this;
    }
    ScdPivotGrid.prototype.componentWillMount = function () {
        var data = this.getData();
        var store = this.getStore(data.data);
        this.installBase = data.instalBase;
        this.matrix = this.getMatrix(store);
        this.setState({ isVisibleEditDialog: false });
    };
    ScdPivotGrid.prototype.render = function () {
        var _this = this;
        return (React.createElement(Container, { fullscreen: true, layout: "fit" },
            React.createElement(PivotGrid, { ref: function (pivotgrid) { return _this.pivotgrid = pivotgrid; }, matrix: this.matrix, store: null, plugins: [
                    this.getConfigurablePlugin(),
                    this.getExportPlugin(),
                    this.getDrillDownPlugin(),
                ], selectable: {
                    mode: 'multi',
                    drag: true,
                    columns: true,
                    cells: true,
                    rows: true,
                    extensible: true,
                }, onSelectionChange: this.onSelectionChange.bind(this) },
                React.createElement(Toolbar, { docked: "top", layout: { type: 'hbox', align: 'stretch' } },
                    React.createElement(Button, { text: "Configurator", handler: function () { return _this.pivotgrid.showConfigurator(); } }),
                    React.createElement(Button, { text: "Export to ..." },
                        React.createElement(Menu, { defaults: { handler: function (menuItem) { return _this.export(menuItem); } } },
                            React.createElement(MenuItem, { text: "Excel", cfg: { type: 'excel07', fileName: 'Pivot.xlsx', onlyExpandedNodes: true } }))),
                    React.createElement(Button, { text: "Edit", handler: function () { return _this.onEditClick(); } }))),
            React.createElement(EditCellsWindow, { isVisible: this.state.isVisibleEditDialog, onEditClick: this.onEditCellDialogClick, onCancelClick: this.onCancelDialogClick })));
    };
    ScdPivotGrid.prototype.updateValues = function (pivotgrid, pivotRecords, startColumnIndex, endColumnIndex, value) {
        var matrix = pivotgrid.getMatrix();
        var matrixColumns = matrix.getColumns();
        var pivotColumns = pivotgrid.getColumns();
        var columnInfos = pivotColumns.filter(function (column, index) { return startColumnIndex <= index && index <= endColumnIndex; })
            .map(function (pivotColumn) { return ({
            dataIndex: pivotColumn.dimension.dataIndex,
            topAxisKey: matrixColumns.find(function (matrixColumn) { return matrixColumn.name === pivotColumn.getDataIndex(); }).col
        }); });
        for (var _i = 0, pivotRecords_1 = pivotRecords; _i < pivotRecords_1.length; _i++) {
            var record = pivotRecords_1[_i];
            for (var _a = 0, columnInfos_1 = columnInfos; _a < columnInfos_1.length; _a++) {
                var columnInfo = columnInfos_1[_a];
                var updater = new Ext.pivot.update.Overwrite({
                    leftKey: record.data.leftAxisKey,
                    topKey: columnInfo.topAxisKey,
                    dataIndex: columnInfo.dataIndex,
                    matrix: matrix,
                    value: value
                });
                updater.update();
            }
        }
    };
    ScdPivotGrid.prototype.getLeftAxisConfig = function () {
        return [
            {
                dataIndex: 'serviceOfferedGroup',
                header: 'Service Offered Group',
                width: 200
            },
            {
                dataIndex: 'warrantyGroup',
                header: 'Warranty Group',
                width: 200
            },
        ];
    };
    ScdPivotGrid.prototype.getTopAxisConfig = function () {
        return [
            {
                dataIndex: 'costBlock',
                header: 'Cost Block',
                width: 200
            },
            {
                dataIndex: 'costElement',
                header: 'Cost Element',
                width: 200
            }
        ];
    };
    ScdPivotGrid.prototype.getMatrix = function (store) {
        return Ext.create('Ext.pivot.matrix.Local', {
            store: store,
            leftAxis: this.getLeftAxisConfig(),
            topAxis: this.getTopAxisConfig(),
            aggregate: [
                {
                    dataIndex: 'costElementValue',
                    header: 'Installation Base',
                    aggregator: this.aggregateInstalationBase.bind(this),
                    renderer: this.aggregateRenderer.bind(this)
                }
            ]
        });
    };
    ScdPivotGrid.prototype.aggregateInstalationBase = function (records, measure, matrix, rowGroupKey, colGroupKey) {
        var result = new AggregateValueInfo();
        var firstRecord = records[0];
        if (records.length === 1) {
            result.value = firstRecord.data[measure];
            result.type = AggregateType.WithoutAggregation;
        }
        else {
            var warrantyGroup_1 = firstRecord.data['warrantyGroup'];
            var serviceOfferedGroup_1 = firstRecord.data['serviceOfferedGroup'];
            var isSameGroups = Ext.Array.every(records, function (record) {
                return record.data['warrantyGroup'] === warrantyGroup_1 &&
                    record.data['serviceOfferedGroup'] === serviceOfferedGroup_1;
            });
            if (isSameGroups) {
                result.value = this.installBase[this.getInstallBaseKeyByRecord(firstRecord)];
                result.type = AggregateType.InstallBaseValue;
            }
            else {
                result.value = this.computeInstallBaseAgregation(records);
                result.type = AggregateType.InstallBaseAggregate;
            }
        }
        return result;
    };
    ScdPivotGrid.prototype.aggregateRenderer = function (value, record, dataIndex, cell, column) {
        var background;
        switch (value.type) {
            case AggregateType.InstallBaseValue:
                cell.setStyle({ background: '#8080802e' });
                break;
            case AggregateType.WithoutAggregation:
                cell.setStyle({ background: '#01d2012b' });
                break;
            default:
                cell.setStyle({ background: null });
                break;
        }
        return Ext.util.Format.number(value.value, '0,000.00');
    };
    ScdPivotGrid.prototype.computeInstallBaseAgregation = function (records) {
        var _this = this;
        var result;
        var installBaseKeys = records.map(function (record) { return _this.getInstallBaseKeyByRecord(record); });
        var installBaseKeySet = new Set(installBaseKeys);
        var installBaseSum = Array.from(installBaseKeySet)
            .reduce(function (prevVal, curVal) { return prevVal + _this.installBase[curVal]; }, 0);
        result =
            records.reduce(function (prevVal, curVal) {
                return prevVal + curVal.data['costElementValue'] / _this.installBase[_this.getInstallBaseKeyByRecord(curVal)];
            }, 0);
        result = result / records.length * installBaseSum;
        return result;
    };
    ScdPivotGrid.prototype.getStore = function (data) {
        return Ext.create('Ext.data.Store', {
            fields: [
                'serviceOfferedGroup',
                'warrantyGroup',
                'costBlock',
                'costElement',
                'costElementValue',
            ],
            data: data
        });
    };
    ScdPivotGrid.prototype.getData = function () {
        var data = [];
        var instalBase = {};
        for (var sogIndex = 1; sogIndex < 5; sogIndex++) {
            for (var wgIndex = 1; wgIndex < 5; wgIndex++) {
                for (var cbIndex = 1; cbIndex < 5; cbIndex++) {
                    for (var ceIndex = 1; ceIndex < 5; ceIndex++) {
                        var serviceOfferedGroup = "Service Offered Group " + sogIndex;
                        var warrantyGroup = "Warranty Group " + wgIndex;
                        data.push({
                            serviceOfferedGroup: serviceOfferedGroup,
                            warrantyGroup: warrantyGroup,
                            costBlock: "Cost Block " + cbIndex,
                            costElement: "Cost Element " + ceIndex,
                            costElementValue: (1000 + sogIndex + wgIndex + cbIndex) * ceIndex,
                        });
                        instalBase[this.getInstallBaseKey(serviceOfferedGroup, warrantyGroup)] = (sogIndex + wgIndex) * 70;
                    }
                }
            }
        }
        console.log("Row count: " + data.length);
        console.log('Instal base:');
        console.log(instalBase);
        return { data: data, instalBase: instalBase };
    };
    ScdPivotGrid.prototype.getInstallBaseKey = function (serviceOfferedGroup, warrantyGroup) {
        return serviceOfferedGroup + "_" + warrantyGroup;
    };
    ScdPivotGrid.prototype.getInstallBaseKeyByRecord = function (record) {
        return this.getInstallBaseKey(record.data['serviceOfferedGroup'], record.data['warrantyGroup']);
        ;
    };
    ScdPivotGrid.prototype.getConfigurablePlugin = function () {
        if (!this.configPlugin) {
            this.configPlugin = Ext.create('Ext.pivot.plugin.Configurator', {
                panelWrap: false,
                fields: [
                    {
                        dataIndex: 'serviceOfferedGroup',
                        header: 'Service Offered Group',
                    },
                    {
                        dataIndex: 'warrantyGroup',
                        header: 'Warranty Group',
                    },
                    {
                        dataIndex: 'costBlock',
                        header: 'Cost Block',
                    },
                    {
                        dataIndex: 'costElement',
                        header: 'Cost Element',
                    }
                ]
            });
        }
        return this.configPlugin;
    };
    ScdPivotGrid.prototype.getExportPlugin = function () {
        if (!this.exportPlugin) {
            this.exportPlugin = Ext.create('Ext.pivot.plugin.Exporter', {});
        }
        return this.exportPlugin;
    };
    ScdPivotGrid.prototype.getDrillDownPlugin = function () {
        if (!this.drillDownPlugin) {
            this.drillDownPlugin = Ext.create('Ext.pivot.plugin.DrillDown', {});
        }
        return this.drillDownPlugin;
    };
    ScdPivotGrid.prototype.getRangeEditorPluging = function () {
        if (!this.rangeEditorPlugin) {
            this.rangeEditorPlugin = Ext.create('Ext.pivot.plugin.RangeEditor', {});
        }
        return this.rangeEditorPlugin;
    };
    ScdPivotGrid.prototype.export = function (_a) {
        var cfg = _a.cfg;
        cfg = __assign({}, cfg, { title: 'Pivot grid' });
        this.pivotgrid.saveDocumentAs(cfg);
    };
    return ScdPivotGrid;
}(React.Component));
export { ScdPivotGrid };
//# sourceMappingURL=ScdPivotGrid.js.map