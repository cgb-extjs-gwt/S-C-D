import * as React from "react";
import { Container, FormPanel, TextField, Grid, Column } from "@extjs/ext-react";
import { Project, ProjectItem } from "../States/Project";
import { Store } from "../../Common/States/ExtStates";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { ProjectItemEditData } from "../States/ProjectCalculatorState";
import { NamedId } from "../../Common/States/CommonStates";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { LocalDynamicGrid } from "../../Common/Components/LocalDynamicGrid";

// const ProjectItemDataIndexes = {
//     wgId: 'wgId',
//     countryId: 'countryId',
//     availability: 'availability',
//     reactionTime: {
//         minutes: 'reactionTime.minutes',
//         periodType: 'reactionTime.periodType'
//     },
//     reactionTypeId: 'reactionTypeId',
//     serviceLocationId: 'serviceLocationId',
//     duration: {
//         months: 'duration.months',
//         periodType: 'duration.periodType'
//     },
//     isRecalculation: 'isRecalculation',
//     onsiteHourlyRates: 'onsiteHourlyRates',
//     fieldServiceCost: {
//         timeAndMaterialShare: 'fieldServiceCost.timeAndMaterialShare',
//         travelCost: 'fieldServiceCost.travelCost',
//         labourCost: 'fieldServiceCost.labourCost',
//         performanceRate: 'fieldServiceCost.performanceRate',
//         travelTime: 'fieldServiceCost.travelTime',
//         oohUpliftFactor: 'fieldServiceCost.oohUpliftFactor',
//     },
//     reinsurance: {
//         flatfee: 'reinsurance.flatfee',
//         upliftFactor: 'reinsurance.upliftFactor',
//         currencyId: 'reinsurance.currencyId'
//     },
//     markupOtherCosts: {
//         markup: 'markupOtherCosts.markup',
//         markupFactor: 'markupOtherCosts.markupFactor',
//         prolongationMarkupFactor: 'markupOtherCosts.prolongationMarkupFactor',
//         prolongationMarkup: 'markupOtherCosts.prolongationMarkup',
//     },
//     logisticsCosts: {
//         expressDelivery: 'logisticsCostsProjCalc.expressDelivery',
//         highAvailabilityHandling: 'logisticsCostsProjCalc.highAvailabilityHandling',
//         standardDelivery: 'logisticsCostsProjCalc.standardDelivery',
//         standardHandling: 'logisticsCostsProjCalc.standardHandling',
//         returnDeliveryFactory: 'logisticsCostsProjCalc.returnDeliveryFactory',
//         taxiCourierDelivery: 'logisticsCostsProjCalc.taxiCourierDelivery',
//     },
//     availabilityFee: {
//         totalLogisticsInfrastructureCost: 'availabilityFee.totalLogisticsInfrastructureCost',
//         stockValueFj: 'availabilityFee.stockValueFj',
//         stockValueMv: 'availabilityFee.stockValueMv',
//         averageContractDuration: 'availabilityFee.averageContractDuration',
//     }
// }

export interface ProjectItemEditorProps {
    project: Project
    projectItemEditData: ProjectItemEditData
}

export class ProjectItemEditor extends React.PureComponent<ProjectItemEditorProps> {
    private form
    private projectItemsGrid: LocalDynamicGrid
    private projectItemsColumnInfos = this.buildProjectItemColumnInfos()
    // private projectItemStore: Store<ProjectItem> = this.createProjectItemStore()

    public render() {
        const { project } = this.props;

        this.form && this.form.setValues(project)
        
        if (this.projectItemsGrid) {
            this.projectItemsGrid.getStore().loadData(project.projectItems)
        }

        return (
            <FormPanel ref={this.setFormRef} layout="vbox">
                <TextField name="creationDate" label="Creation date" readOnly={true}/>
                <TextField name="user.name" label="User" readOnly={true}/>
                <TextField name="name" label="Project name"/>

                <LocalDynamicGrid 
                    ref={this.setProjectItemsGridRef} 
                    columns={this.projectItemsColumnInfos}
                    getSaveToolbar={this.getToolbar}
                />
            </FormPanel>
        )
    }

    private setFormRef = form => {
        this.form = form
    }

    private setProjectItemsGridRef = grid => {
        this.projectItemsGrid = grid
    }

    private getToolbar() {
        return <div/>
    }

    // private createProjectItemStore(): Store<Project> {
    //     return Ext.create('Ext.data.Store', {
    //         fields: [ 
    //             { name: DataIndexes.name, type: '' },
    //         ]
    //     });
    // }

    private buildProjectItemColumnInfos() {
        const { projectItemEditData } = this.props;
        const columns: ColumnInfo<ProjectItem>[] = [
            buildReferenceColumn('wgId', 'Wg', projectItemEditData.wgs),
            buildReferenceColumn('countryId', 'Country', projectItemEditData.wgs),
            //availability
            buildCombinedColumn('reactionTime', 'Reaction Time', [
                buildNumericColumn('value', 'Value'),
                buildReferenceColumn('periodType', 'Period', projectItemEditData.reactionTimePeriods)
            ]),
            buildReferenceColumn('countryId', 'Country', projectItemEditData.wgs),
            buildReferenceColumn('reactionTypeId', 'Reaction Type', projectItemEditData.reactionTypes),
            buildReferenceColumn('serviceLocationId', 'Service Location', projectItemEditData.serviceLocations),
            buildCombinedColumn('duration', 'Duration', [
                buildNumericColumn('value', 'Value'),
                buildReferenceColumn('periodType', 'Period', projectItemEditData.durationPeriods)
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
                buildReferenceColumn('currencyId', 'Currency', projectItemEditData.reinsuranceCurrencies)
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

        function buildNumericColumn(dataIndex: string, title: string): ColumnInfo<ProjectItem> {
            return {
                dataIndex,
                title,
                isEditable: true,
                type: ColumnType.Numeric
            }
        }

        function buildReferenceColumn(dataIndex: string, title: string, referenceItems: NamedId<number>[]): ColumnInfo<ProjectItem> {
            const map = new Map<number, NamedId<number>>();

            referenceItems.forEach(item => map.set(item.id, item))

            return {
                dataIndex,
                title,
                type: ColumnType.Reference,
                referenceItems: map
            }
        }

        function buildCombinedColumn(dataIndex: string, title: string, columns: ColumnInfo<ProjectItem>[]): ColumnInfo<ProjectItem> {
            columns = columns.map(column => {
                const nestedDataIndex = `${dataIndex}.${column.dataIndex}`

                return {
                    ...column,
                    dataIndex: nestedDataIndex,
                    mappingFn: item => item[dataIndex][column.dataIndex],
                    editMappingFn: record => record.data[dataIndex][column.dataIndex] = record.get(nestedDataIndex)
                }
            })
            
            return {
                dataIndex,
                title,
                columns
            }
        }
    }
}