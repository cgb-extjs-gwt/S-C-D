import * as React from "react";
import { AjaxDynamicGrid, AjaxDynamicGridProps, ApiUrls } from "../../Common/Components/AjaxDynamicGrid";
import { LocalDynamicGridActions } from "../../Common/Components/LocalDynamicGrid";
import { TableViewRecord } from "../States/TableViewRecord";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { DynamicGridActions } from "../../Common/Components/Props/DynamicGridProps";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { Model } from "../../Common/States/ExtStates";
import { Container, Toolbar, Button } from "@extjs/ext-react";
import { HistoryButtonView } from "../../History/Components/HistoryButtonView";
import { QualtityGateSetWindowContainer } from "./QualtityGateSetWindowContainer";
import { RouteComponentProps } from "react-router";

Ext.require('Ext.grid.plugin.Exporter')

export interface TableViewGridActions extends LocalDynamicGridActions<TableViewRecord>, DynamicGridActions {
    onApprove?()
    onSelectionChange?(grid, records: Model[], selecting: boolean, selectionInfo)
}

export interface TableViewGridProps extends TableViewGridActions, RouteComponentProps {
    columns: ColumnInfo[]
    apiUrls: ApiUrls
    hasChanges: boolean
    buildHistotyDataLoadUrl(selection: Model<TableViewRecord>[], selectedDataIndex: string): string
}

export interface TableViewGridState {
    selection: Model<TableViewRecord>[]
    selectedDataIndex: string
    isEnableHistoryButton: boolean
}

export class TableViewGrid extends React.Component<TableViewGridProps, TableViewGridState> {
    private innerGrid: AjaxDynamicGrid

    constructor(props) {
        super(props)

        this.state = {
            selection: [],
            selectedDataIndex: null,
            isEnableHistoryButton: false
        };
    }

    public componentWillReceiveProps(nextProps: TableViewGridProps) {
        if (this.innerGrid && !nextProps.hasChanges) {
            this.innerGrid.commitChanges();
        }
    }

    public shouldComponentUpdate(nextProps: TableViewGridProps, nextState: TableViewGridState) {
        return (
            this.state != nextState ||
            this.props != nextProps && 
            (this.props.apiUrls != nextProps.apiUrls || 
            this.props.columns != nextProps.columns ||
            this.props.buildHistotyDataLoadUrl != nextProps.buildHistotyDataLoadUrl)
        );
    }

    public render() {
        const gridProps = this.props as AjaxDynamicGridProps;
        const { isEnableHistoryButton } = this.state;

        return (
            <Container layout="fit">
                <Toolbar docked="top">
                    <HistoryButtonView
                        isEnabled={isEnableHistoryButton}
                        flex={1}
                        windowPosition={{
                            top: '300',
                            left: '25%'
                        }}
                        buidHistoryUrl={this.buidHistoryUrl}
                    />
                    <Button text="Export to Excel" flex={1} handler={this.onExportToExcel}/>
                    <QualtityGateSetWindowContainer position={{ top: '25%', left: '25%'}}/>
                </Toolbar>

                <AjaxDynamicGrid 
                    { ...gridProps } 
                    ref={this.innerGridRef}
                    getSaveToolbar={this.getSaveToolbar}
                    onSelectionChange={this.onSelectionChange} 
                />
            </Container>
        );
    }

    private buidHistoryUrl = () => {
        const { buildHistotyDataLoadUrl } = this.props;
        const { selection, selectedDataIndex } = this.state;

        return buildHistotyDataLoadUrl && buildHistotyDataLoadUrl(selection, selectedDataIndex);
    }

    private onExportToExcel = () => {
        this.innerGrid.exportToExcel('CentralDataInput.xlsx');
    } 

    private innerGridRef = (grid: AjaxDynamicGrid) => {
        this.innerGrid = grid;
    }

    private getSaveToolbar = (
        hasChanges: boolean, 
        ref: (toolbar: SaveToolbar) => void, 
        { cancel, save, saveWithCallback }: DynamicGrid
    ) => {
        const { onApprove, onSave } = this.props;

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

    private onSelectionChange = (grid, records: Model<TableViewRecord>[], selecting: boolean, selectionInfo) => {
        const { startCell } = selectionInfo;

         if (startCell) {
            const column = selectionInfo.startCell.column;
            const dataIndex = column.getDataIndex();

            this.setState({
                selection: records,
                selectedDataIndex: dataIndex,
                isEnableHistoryButton: dataIndex in records[0].data.data
            });
         }
         else {
            this.setState({
                selection: [],
                selectedDataIndex: null,
                isEnableHistoryButton: false
            });
         }
    }
}