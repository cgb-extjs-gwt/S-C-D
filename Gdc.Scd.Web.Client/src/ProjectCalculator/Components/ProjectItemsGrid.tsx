import * as React from "react";
import { ProjectItem, DayOfWeek, AvailabilityProjCalc } from "../States/Project";
import { NamedId } from "../../Common/States/CommonStates";
import { ColumnType, ColumnInfo } from "../../Common/States/ColumnInfo";
import { ProjectItemEditData } from "../States/ProjectCalculatorState";
import { LocalDynamicGrid } from "../../Common/Components/LocalDynamicGrid";
import { Store, Model, StoreOperation } from "../../Common/States/ExtStates";
import { Container, Toolbar, Button, Dialog } from "@extjs/ext-react";
import { buildGetReferenceNameFn } from "../../Common/Helpers/GridHeper";
import { AvailabilityEditor } from "./AvailabilityEditor";

export interface ProjectItemsGridActions {
    onAddRecord?(store: Store<ProjectItem>, records: Model<ProjectItem>[])
    onRemoveRecord?(store: Store<ProjectItem>, records: Model<ProjectItem>[])
    onUpdateRecordSet?(records: Model<ProjectItem>[], operation: StoreOperation, dataIndex: string)
}

export interface ProjectItemsGridProps extends ProjectItemsGridActions {
    projectItems: ProjectItem[]
    projectItemEditData: ProjectItemEditData    
}

export interface ProjectItemsGridState {
    selectedItems: Model<ProjectItem>[]
    isDisplayedAvailabilityEditor: boolean
}

export class ProjectItemsGrid extends React.PureComponent<ProjectItemsGridProps, ProjectItemsGridState> {
    private columnInfos: ColumnInfo<ProjectItem>[]
    private grid: LocalDynamicGrid<ProjectItem>
    private fakeId = 0
    
    constructor(props: ProjectItemsGridProps) {
        super(props)

        if (props.projectItemEditData) {
            this.columnInfos = this.buildColumnInfos(props.projectItemEditData);
        }

        this.state = {
            selectedItems: [],
            isDisplayedAvailabilityEditor: false
        }
    }

    public componentWillReceiveProps(nextProps: ProjectItemsGridProps) {
        if (this.props.projectItemEditData != nextProps.projectItemEditData) {
            this.columnInfos = this.buildColumnInfos(nextProps.projectItemEditData);
        }
    }

    public componentDidUpdate(prevProps: ProjectItemsGridProps) {
        if (prevProps.projectItemEditData != this.props.projectItemEditData ||
            prevProps.projectItems != this.props.projectItems) {

            if (this.grid) {
                const projectItems = this.props.projectItems || [];
                const store: Store<ProjectItem> = this.grid.getStore();

                store.removeAll();
                store.loadData(projectItems);
            }
        }
    }

    public render() {
        const { selectedItems, isDisplayedAvailabilityEditor } = this.state;
        const selecttedItem = selectedItems.length == 0 ? null : selectedItems[0];

        return (
            <Container layout="vbox" flex={1}>
                <Toolbar layout="hbox" docked="top">
                    <Button text="Add" handler={this.add} flex={1}/>
                    <Button text="Delete" handler={this.delete} flex={1} disabled={!selecttedItem}/>
                </Toolbar>  
                {
                    this.columnInfos &&
                    <LocalDynamicGrid
                        ref={this.setGridRef} 
                        flex={1}
                        columns={this.columnInfos}
                        getSaveToolbar={this.getToolbar}
                        onAddRecord={this.onAddRecord}
                        onRemoveRecord={this.onRemoveRecord}
                        onUpdateRecordSet={this.onUpdateRecordSet}
                        onSelectionChange={this.onSelectionChange}
                    />
                }
                {
                    isDisplayedAvailabilityEditor &&
                    <Dialog 
                        displayed={isDisplayedAvailabilityEditor}
                        title="Availability" 
                        closable 
                        layout="fit"
                        maximizable
                        resizable={{
                            dynamic: true,
                            edges: 'all'
                        }}
                        width="60%"
                        height="80%"
                        onClose={this.hideAvailabilityEditor}
                    >
                        <AvailabilityEditor 
                            availability={selecttedItem.data.availability} 
                            onSave={this.saveAvailability}
                            onClose={this.hideAvailabilityEditor}
                        />
                    </Dialog>
                }
            </Container>
        )
    }

    public getEditedProjectItems = () => {
        let projectItems : ProjectItem[] = null;

        if (this.grid) {
            projectItems = [];

            this.grid.getStore().each(record => {
                projectItems.push({ 
                    ...record.data, 
                    id: record.data.id < 0 ? 0 : record.data.id,    
                });
            });
        }

        return projectItems;
    }

    private showAvailabilityEditor = () => {
        if (this.state.selectedItems.length == 1) {
            this.setState({isDisplayedAvailabilityEditor: true })
        }
    }

    private hideAvailabilityEditor = () => {
        this.setState({isDisplayedAvailabilityEditor: false })
    }

    private saveAvailability = (availability: AvailabilityProjCalc) => {
        this.state.selectedItems[0].set('availability', availability);
        this.hideAvailabilityEditor();
    }

    private add = () => {
        const projectItem: ProjectItem = { 
            id: --this.fakeId,
            isRecalculation: true,
            availability: {
                start: null,
                end: null
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

    private setGridRef = grid => {
        this.grid = grid
    }

    private getToolbar() {
        return <div/>
    }

    private onUpdateRecordSet = (
        records: Model<ProjectItem>[], 
        operation: StoreOperation, 
        dataIndex: string
    ) => {
        const { onUpdateRecordSet: onUpdateRecord } = this.props;

        onUpdateRecord && onUpdateRecord(records, operation, dataIndex);
    }

    private onAddRecord = (store, records) => {
        const { onAddRecord } = this.props;

        onAddRecord && onAddRecord(store, records);
    }

    private onRemoveRecord = (store, records) => {
        const { onRemoveRecord } = this.props;

        onRemoveRecord && onRemoveRecord(store, records);
    }

    private buildColumnInfos(projectItemEditData: ProjectItemEditData) {
        const me = this;
        const columns: ColumnInfo<ProjectItem>[] = [
            buildReferenceColumn('wgId', 'Wg', projectItemEditData.wgs),
            buildReferenceColumn('countryId', 'Country', projectItemEditData.countries),
            buildAvailabilityColumn(),
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

        function buildAvailabilityColumn(): ColumnInfo<ProjectItem> {
            return {
                dataIndex: 'availability',
                title: 'Availability',
                type: ColumnType.Button,
                width: 230,
                isEditable: true,
                buttonHandler: () => {
                    me.showAvailabilityEditor();
                },
                rendererFn: (value, record, dataIndex, cell) => {
                    let name: string = ' ';

                    const { availability } = record.data;

                    if (availability) {
                        const { start, end } = availability;

                        if (start && end) {
                            name = `${DayOfWeek[start.day]} - ${DayOfWeek[end.day]} (${start.hour}:00-${end.hour + 1}:00)`;

                            setRequiredStyle(false, cell);
                        } else {
                            setRequiredStyle(true, cell);
                        }
                    } else {
                        setRequiredStyle(true, cell);
                    }

                    return name;
                }
            }
        }

        function setRequiredStyle(isAcitve: boolean, cell) {
            if (cell) {
                cell.setStyle({ 
                    background: isAcitve ? 'rgba(194, 6, 6, 1)' : null
                });
            }
        }

        function buildColumn(dataIndex: string, title: string, isRequired = false, isEditable = true, width = null): ColumnInfo<ProjectItem> {
            const nullValueRenderer = value => value == null ? ' ' : value;
            const requiredRenderer = (value, record, dataIndex, cell) => {
                setRequiredStyle(value == null, cell);

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