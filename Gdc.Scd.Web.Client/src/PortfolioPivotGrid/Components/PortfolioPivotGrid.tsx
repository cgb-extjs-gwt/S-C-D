import * as React from 'react';
import { Container, Toolbar, Button } from '@extjs/ext-react';
import { PivotGrid } from '@extjs/ext-react-pivot';
import { buildMvcUrl } from '../../Common/Services/Ajax';
import { FilterPanel } from '../../Portfolio/Components/FilterPanel';

export class PortfolioPivotGrid extends React.Component {
    private readonly configuratorPlugin;

    private readonly matrix;

    private pivotGrid: PivotGrid & any; 

    private filter: FilterPanel;

    constructor(props) {
        super(props)

        this.configuratorPlugin = this.getConfiguratorPlugin();
        this.matrix = this.getMatrix();
    }

    render() {
        return (
            <Container layout="fit">
                <Toolbar docked="top" padding="5 8">
                    <Button text="Configurator" handler={this.showConfigurator}/>
                </Toolbar>
                <PivotGrid 
                    ref={this.refPivotGrid}
                    matrix={this.matrix} 
                    store={null} 
                    plugins={[this.configuratorPlugin]}
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
}