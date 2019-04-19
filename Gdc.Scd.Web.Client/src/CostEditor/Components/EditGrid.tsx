import * as React from "react";
import { Grid, SelectField, Column, Container, CheckBoxField, CheckColumn, NumberField, ComboBoxField } from "@extjs/ext-react";
import { EditItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { large, small } from "../../responsiveFormulas";
import { FieldType, InputType } from "../../Common/States/CostMetaStates";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { buildCostElementColumn } from "../../Common/Helpers/ColumnInfoHelper";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { StoreUpdateEventFn } from "../../Common/States/ExtStates";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { AjaxDynamicGrid, ApiUrls, AjaxDynamicGridProps } from "../../Common/Components/AjaxDynamicGrid";
import { objectPropsEqual } from "../../Common/Helpers/CommonHelpers";

export interface ValueColumnProps {
    title: string
    type: FieldType,
    selectedItems: NamedId<number>[]
    inputType: InputType
    currency?: string
}

export interface EditGridActions {
    onItemEdited?(item: EditItem)
    onSelected?(items: EditItem[])
    onApprove?()
    onCancel?()
    onSave?()
}

export interface EditGridProps extends EditGridActions {
    nameColumnTitle: string
    valueColumn: ValueColumnProps
    url: string
    hasChanges: boolean
}

export class EditGrid extends React.Component<EditGridProps> {
    private innerGrid: AjaxDynamicGrid

    public shouldComponentUpdate(nextProps: EditGridProps) {
        return (
            this.props.url != nextProps.url ||
            this.props.valueColumn.currency != nextProps.valueColumn.currency
        );
    }

    public componentWillReceiveProps(nextProps: EditGridProps) {
        if (this.innerGrid && !nextProps.hasChanges) {
            this.innerGrid.commitChanges();
        }
    }

    public render() {
        const { url, valueColumn, nameColumnTitle } = this.props;
        const columns = this.buildColumnInfos(nameColumnTitle, valueColumn);

        return (
            url &&
            <AjaxDynamicGrid 
                flex={1}
                ref={this.innerGridRef}
                columns={columns} 
                apiUrls={{ read: url }}
                getSaveToolbar={this.getSaveToolbar}
                onSelectionChange={this.onSelected}
                onUpdateRecord={this.onUpdateRecord}
                onCancel={this.onCancel}
            />
        );
    }

    private innerGridRef = (grid: AjaxDynamicGrid) => {
        this.innerGrid = grid;
    }

    private buildColumnInfos(nameColumnTitle: string, valueColumn: ValueColumnProps) {
        return [
            { 
                title: nameColumnTitle, 
                dataIndex: "name", 
                extensible: false,
                flex: 1, 
            },
            buildCostElementColumn<EditItem>({
                title: valueColumn.title,
                dataIndex: "value",
                type: valueColumn.type,
                references: valueColumn.selectedItems,
                inputType: valueColumn.inputType,
                flex: 1,
                currency: valueColumn.currency,
                getCountFn: ({ data }) => data.valueCount,
                getIsApprovedFn: ({ data }) => data.isApproved
            })
        ] as ColumnInfo[]
    }

    private getSaveToolbar = (
        hasChanges: boolean, 
        ref: (toolbar: SaveToolbar) => void,
        { cancel }: DynamicGrid
    ) => {
        const { onSave, onApprove } = this.props;

        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={cancel}
                onSave={onSave}
                onApproval={onApprove}
            />
        );
    }

    private onSelected = (grid, records: { data: EditItem }[]) => {
        const { onSelected } = this.props;

        if (onSelected) {
            const editItems = records ? records.map(record => record.data) : [];

            onSelected(editItems);
        }
    }

    private onUpdateRecord: StoreUpdateEventFn<EditItem> = (store, record, operation, modifiedFieldNames, details) => {
        const { onItemEdited } = this.props;

        if (modifiedFieldNames) {
            if (modifiedFieldNames[0] === 'name') {
                record.reject();
            } else {
                if (record.data.value == null) {
                    record.set({ 
                        valueCount: 0,
                        value: undefined,
                        isApproved: false
                    });
                } else {
                    record.set({ 
                        valueCount: 1,
                        isApproved: false
                    });
                }
    
                onItemEdited(record.data);
            }
        }
    }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave();
    }

    private onCancel = () => {
        const { onCancel } = this.props;

        onCancel && onCancel();
    }
}
