import * as React from "react";
import { ProjectItem } from "../States/Project";
import { NamedId } from "../../Common/States/CommonStates";
import { ColumnType, ColumnInfo } from "../../Common/States/ColumnInfo";
import { ProjectItemEditData } from "../States/ProjectCalculatorState";
import { LocalDynamicGrid } from "../../Common/Components/LocalDynamicGrid";
import { StoreUpdateEventFn, Store, Model, StoreOperation } from "../../Common/States/ExtStates";
import { Container, Toolbar, Button } from "@extjs/ext-react";
import { buildReferenceColumnRendered, buildGetReferenceNameFn } from "../../Common/Helpers/GridHeper";

export interface ProjectItemsGridActions {
    onUpdateRecord?: StoreUpdateEventFn<ProjectItem>
}

export interface ProjectItemsGridProps extends ProjectItemsGridActions {
    projectItems: ProjectItem[]
    projectItemEditData: ProjectItemEditData    
}

export interface ProjectItemsGridState {
    selectedItems: Model<ProjectItem>[]
}

export class ProjectItemsGrid extends React.PureComponent<ProjectItemsGridProps, ProjectItemsGridState> {
    private columnInfos: ColumnInfo<ProjectItem>[]
    private grid: LocalDynamicGrid<ProjectItem>
    private fakeId = 0
    
    constructor(props: ProjectItemsGridProps) {
        super(props)

        this.state = {
            selectedItems: []
        }
    }

    public componentWillUpdate(nextProps: ProjectItemsGridProps) {
        if (this.props.projectItemEditData != nextProps.projectItemEditData) {
            this.columnInfos = this.buildColumnInfos(nextProps.projectItemEditData);
        }
    }

    public componentDidUpdate(prevProps: ProjectItemsGridProps) {
        if (prevProps.projectItemEditData != this.props.projectItemEditData ||
            prevProps.projectItems != this.props.projectItems) {

            if (this.grid) {
                const projectItems = this.props.projectItems || [];

                this.grid.getStore().loadData(projectItems);
            }
        }
    }

    public render() {
        const disabled = this.state.selectedItems.length == 0;

        return (
            <Container layout="vbox" flex={1}>
                <Toolbar layout="hbox" docked="top">
                    <Button text="Add" handler={this.add} flex={1}/>
                    <Button text="Delete" handler={this.delete} flex={1} disabled={disabled}/>
                </Toolbar>  
                {
                    this.columnInfos &&
                    <LocalDynamicGrid
                        ref={this.setGridRef} 
                        flex={1}
                        columns={this.columnInfos}
                        getSaveToolbar={this.getToolbar}
                        onUpdateRecord={this.onUpdateRecord}
                        onSelectionChange={this.onSelectionChange}
                    />
                }
            </Container>
        )
    }

    private add = () => {
        const projectItem: ProjectItem = { 
            id: --this.fakeId,
            isRecalculation: true,
            availability: {
                start: {},
                end: {}
            },
            duration: {},
            reactionTime: {},
            availabilityFee: {},
            fieldServiceCost: {},
            reinsurance: {},
            logisticsCosts: {},
            markupOtherCosts: {},
        }

        this.grid.getStore().add(projectItem);
    }

    private delete = () => {
        this.grid.getStore().remove(this.state.selectedItems)
    }

    private onSelectionChange = (grid, records: Model<ProjectItem>[], selecting: boolean, selectionInfo) => {
        this.setState({ selectedItems:  records});
    }

    public getEditedProjectItems = () => {
        const projectItems : ProjectItem[] = [];

        this.grid.getStore().each(record => {
            projectItems.push({ 
                ...record.data, 
                id: record.data.id < 0 ? 0 : record.data.id,    
            });
        });

        return projectItems;
    }

    private setGridRef = grid => {
        this.grid = grid
    }

    private getToolbar() {
        return <div/>
    }

    private onUpdateRecord = (
        store: Store<ProjectItem>, 
        record: Model<ProjectItem>, 
        operation: StoreOperation, 
        modifiedFieldNames: string[], 
        details
    ) => {
        const { onUpdateRecord } = this.props;

        onUpdateRecord && onUpdateRecord(store, record, operation, modifiedFieldNames, details);
    }

    private buildColumnInfos(projectItemEditData: ProjectItemEditData) {
        const columns: ColumnInfo<ProjectItem>[] = [
            buildReferenceColumn('wgId', 'Wg', projectItemEditData.wgs),
            buildReferenceColumn('countryId', 'Country', projectItemEditData.countries),
            //availability
            buildCombinedColumn('reactionTime', 'Reaction Time', [
                buildNumericColumn('value', 'Value'),
                buildReferenceColumn('periodType', 'Period', projectItemEditData.reactionTimePeriods, false, true)
            ]),
            buildReferenceColumn('reactionTypeId', 'Reaction Type', projectItemEditData.reactionTypes),
            buildReferenceColumn('serviceLocationId', 'Service Location', projectItemEditData.serviceLocations, true, false, 200),
            buildCombinedColumn('duration', 'Duration', [
                buildNumericColumn('value', 'Value', true),
                buildReferenceColumn('periodType', 'Period', projectItemEditData.durationPeriods, true, true)
            ]),
            buildNumericColumn('onsiteHourlyRates', 'Onsite Hourly Rates'),
            buildCombinedColumn('fieldServiceCost', 'Field Service Cost', [
                buildNumericColumn('timeAndMaterialShare', 'Time And Material Share'),
                buildNumericColumn('travelCost', 'Travel Cost'),
                buildNumericColumn('labourCost', 'LabourCost'),
                buildNumericColumn('performanceRate', 'Performance Rate'),
                buildNumericColumn('travelTime', 'Travel Time'),
                buildNumericColumn('oohUpliftFactor', 'Ooh Uplift Factor'),
            ]),
            buildCombinedColumn('reinsurance', 'Reinsurance', [
                buildNumericColumn('flatfee', 'Flat Fee'),
                buildNumericColumn('upliftFactor', 'Uplift Factor'),
                buildReferenceColumn('currencyId', 'Currency', projectItemEditData.reinsuranceCurrencies, false, true)
            ]),
            buildCombinedColumn('markupOtherCosts', 'Markup Other Costs', [
                buildNumericColumn('markup', 'Markup'),
                buildNumericColumn('markupFactor', 'MarkupFactor'),
                buildNumericColumn('prolongationMarkupFactor', 'Prolongation Markup Factor'),
                buildNumericColumn('prolongationMarkup', 'Prolongation Markup'),
            ]),
            buildCombinedColumn('logisticsCosts', 'Logistics Costs', [
                buildNumericColumn('expressDelivery', 'Express Delivery'),
                buildNumericColumn('highAvailabilityHandling', 'High Availability Handling'),
                buildNumericColumn('standardDelivery', 'Standard Delivery'),
                buildNumericColumn('standardHandling', 'Standard Handling'),
                buildNumericColumn('returnDeliveryFactory', 'ReturnDeliveryFactory'),
                buildNumericColumn('taxiCourierDelivery', 'Taxi Courier Delivery'),
            ]),
            buildCombinedColumn('availabilityFee', 'Availability Fee', [
                buildNumericColumn('totalLogisticsInfrastructureCost', 'Total Logistics Infrastructure Cost'),
                buildNumericColumn('stockValueFj', 'Stock Value Fj'),
                buildNumericColumn('stockValueMv', 'Stock Value Mv'),
                buildNumericColumn('averageContractDuration', 'Average Contract Duration'),
            ]),
        ];

        return columns

        function buildColumn(dataIndex: string, title: string, isRequired = false, isEditable = true, width = null): ColumnInfo<ProjectItem> {
            const nullValueRenderer = value => value == null ? ' ' : value;
            const requiredRenderer = (value, record, dataIndex, cell) => {
                cell.setStyle({ 
                    background: value == null ? 'rgba(194, 6, 6, 1)' : null
                });

                return nullValueRenderer(value)
            }

            return {
                dataIndex,
                title,
                isEditable,
                width,
                rendererFn: isRequired ? requiredRenderer : nullValueRenderer
            };
        }

        function buildNumericColumn(dataIndex: string, title: string, isRequired = false): ColumnInfo<ProjectItem> {
            return {
                ...buildColumn(dataIndex, title, isRequired),
                type: ColumnType.Numeric
            }
        }

        function buildReferenceColumn(dataIndex: string, title: string, referenceItems: NamedId<number>[], isRequired = true, isNested = false, width = 150): ColumnInfo<ProjectItem> {
            const map = new Map<number, NamedId<number>>();

            referenceItems.forEach(item => map.set(item.id, item));

            const column = { 
                ...buildColumn(dataIndex, title, isRequired,)
            };

            const referenceColumn: ColumnInfo<ProjectItem> = {
                ...column,
                referenceItems: map,
                type: ColumnType.Reference,
                width
            }

            if (isNested) {
                const getReferenceName = buildGetReferenceNameFn(map);

                referenceColumn.rendererFn = (value, record, dataIndex, cell) => {
                    value = column.rendererFn(value, record, dataIndex, cell);

                    return getReferenceName(value);
                }
            }

            return referenceColumn
        }

        function buildCombinedColumn(topDataIndex: string, title: string, columns: ColumnInfo<ProjectItem>[]): ColumnInfo<ProjectItem> {
            columns = columns.map(column => {
                const nestedDataIndex = `${topDataIndex}.${column.dataIndex}`

                return {
                    ...column,
                    dataIndex: nestedDataIndex,
                    rendererFn: (val, record, dataIndex, cell) => {
                        const value = record.data[topDataIndex][column.dataIndex];

                        return column.rendererFn(value, record, dataIndex, cell);
                    },
                    editMappingFn: record => record.data[topDataIndex][column.dataIndex] = record.get(nestedDataIndex)
                } as ColumnInfo<ProjectItem>
            })
            
            return {
                ...buildColumn(topDataIndex, title),
                columns
            }
        }
    }
}