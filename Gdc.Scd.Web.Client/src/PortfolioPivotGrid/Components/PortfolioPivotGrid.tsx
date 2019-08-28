import * as React from 'react';
import { Container, Toolbar, Button, Menu, MenuItem } from '@extjs/ext-react';
import { PivotGrid } from '@extjs/ext-react-pivot';
import { buildMvcUrl } from '../../Common/Services/Ajax';
import { FilterPanel } from '../../Portfolio/Components/FilterPanel';
import { pivotExcelExport } from '../Services/PortfolioPivotGridService';
import { PortfolioPivotRequest, RequestAxisItem } from '../States/PortfolioPivotRequest';

Ext.require(['Ext.exporter.excel.PivotXlsx'])

export class PortfolioPivotGrid extends React.Component {
    private readonly configuratorPlugin;

    private readonly exporterPlugin;

    private readonly matrix;

    private pivotGrid: PivotGrid & any; 

    private filter: FilterPanel;

    constructor(props) {
        super(props)

        this.configuratorPlugin = this.getConfiguratorPlugin();
        this.exporterPlugin = Ext.create('Ext.pivot.plugin.Exporter');
        this.matrix = this.getMatrix();
    }

    render() {
        return (
            <Container layout="fit">
                <Toolbar docked="top" padding="5 8">
                    <Button text="Configurator" handler={this.showConfigurator}/>
                    <Button text="Export to ...">
                        <Menu defaults={{ iconCls: 'x-fa fa-file-text-o' }}>
                            <MenuItem 
                                text="Excel xlsx (pivot table definition)" 
                                iconCls="x-fa fa-file-excel-o"
                                handler={this.exportPivotExcel}
                                cfg={{
                                    type: 'pivotxlsx',
                                    fileName: 'ExportPivot.xlsx'
                                }}
                            />
                            <MenuItem
                                text="Excel xlsx (all items)"
                                iconCls="x-fa fa-file-excel-o"
                                handler={this.exportExcel}
                                cfg={{
                                    type: 'excel07',
                                    fileName: 'ExportAll.xlsx'
                                }}
                            />
                            <MenuItem
                                text="Excel xlsx (visible items)"
                                iconCls="x-fa fa-file-excel-o"
                                handler={this.exportExcel}
                                cfg={{
                                    type: 'excel07',
                                    fileName: 'ExportVisible.xlsx',
                                    onlyExpandedNodes: true
                                }}
                            />
                        </Menu>
                    </Button>
                </Toolbar>
                <PivotGrid 
                    ref={this.refPivotGrid}
                    matrix={this.matrix} 
                    store={null} 
                    plugins={[ this.configuratorPlugin, this.exporterPlugin]}
                />
                <FilterPanel 
                    ref={this.refFilter}
                    docked="right" 
                    onSearch={this.applyFilter} 
                    scrollable={true} 
                    isCountryUser={true} 
                />
            </Container>
        );
    }

    private axisRenderer(value) {
        return value == null ? ' ' : value;
    }

    private getLeftAxis() {
        return [
            {
                id: 'ServiceLocation',
                dataIndex: 'ServiceLocationId',
                header: 'Service location',
                renderer: this.axisRenderer
            },
            {
                id: 'ReactionTime',
                dataIndex: 'ReactionTimeId',
                header: 'Reaction time',
                renderer: this.axisRenderer
            },
            {
                id: 'ReactionType',
                dataIndex: 'ReactionTypeId',
                header: 'Reaction type',
                renderer: this.axisRenderer
            },
            {
                id: 'Availability',
                dataIndex: 'AvailabilityId',
                header: 'Availability',
                renderer: this.axisRenderer
            },
            {
                id: 'ProActive',
                dataIndex: 'ProActiveSlaId',
                header: 'ProActive',
                renderer: this.axisRenderer
            },
        ]
    }

    private getTopAxis() {
        return [
            {
                id: 'Sog',
                dataIndex: 'SogId',
                header: 'Sog',
                renderer: this.axisRenderer
            },
            {
                id: 'Wg',
                dataIndex: 'WgId',
                header: 'Wg',
                renderer: this.axisRenderer
            },
        ]
    }

    private getAggregateItems() {
        return [{
            id: 'count',
            dataIndex: 'count',
            header: 'Count',
            aggregator: 'count',
            renderer: aggregateRenderer
        }]

        function aggregateRenderer(value) {
            return value == null ? 0 : value;
        }
    }

    private getMatrix() {
        return Ext.create('Ext.pivot.matrix.Remote', {
            type: 'remote',
            url: buildMvcUrl('PortfolioPivotGrid', 'GetData'),
            timeout: 600000,
            rowGrandTotalsPosition: 'none',
            colGrandTotalsPosition: 'none',
            colSubTotalsPosition: 'none',
            aggregate: this.getAggregateItems(),
            leftAxis: this.getLeftAxis().filter((item, index) => index < 3),
            topAxis: this.getTopAxis(),
            listeners: {
                beforerequest: (matrix, params) => {
                    params.filter = this.filter.getModel();
                }
            }
        });
    }

    private refPivotGrid = (grid: PivotGrid) => {
        this.pivotGrid = grid;
    }

    private refFilter = (filter: FilterPanel) => {
        this.filter = filter;
    }

    private showConfigurator = () => {
        this.pivotGrid.showConfigurator();
    }

    private getConfiguratorPlugin() {
        const axisItems = [
            ...this.getLeftAxis(), 
            ...this.getTopAxis()
        ];

        const axisFields = axisItems.map(item => ({
            ...item,
            aggregator: 'count',
            settings: {
                allowed: ['leftAxis', 'topAxis']
            }
        }));

        const aggregateFields = this.getAggregateItems().map(item => ({
            ...item,
            settings: {
                fixed: ['aggregate']
            }
        }));

        return Ext.create('Ext.pivot.plugin.Configurator', {
            panelWrap: false,
            fields: [
                ...axisFields,
                ...aggregateFields
            ]
        });
    }

    private applyFilter = () => {
        this.pivotGrid.reconfigurePivot();
    }

    private exportDocument = config => {
        config = {
            ...config,
            title: 'Pivot grid export'
        };   

        this.pivotGrid.saveDocumentAs(config);
    }

    private exportExcel = menuItem => {
        this.exportDocument(menuItem.cfg);
    }

    private exportPivotExcel = menuItem => {
        const request = this.getPortfolioPivotRequest();

        pivotExcelExport(request);
    }

    private getPortfolioPivotRequest(): PortfolioPivotRequest {
        const { keysSeparator, grandTotalKey, aggregate, leftAxis, topAxis} = this.matrix;

        return {
            keysSeparator,
            grandTotalKey,
            aggregate: aggregate.items.map(buildAxisItem),
            leftAxis: leftAxis.dimensions.items.map(buildAxisItem),
            topAxis: topAxis.dimensions.items.map(buildAxisItem),
            filter: this.filter.getModel()
        };

        function buildAxisItem(obj): RequestAxisItem {
            return {
                id: obj.id,
                dataIndex: obj.dataIndex,
                aggregator: obj.aggregator,
                header: obj.header,
                direction: obj.direction
            };
        }
    }
}