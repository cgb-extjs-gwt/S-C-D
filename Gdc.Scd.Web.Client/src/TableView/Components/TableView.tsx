import * as React from "react";
import { AjaxDynamicGridActions } from "../../Common/Components/AjaxDynamicGrid";
import { Container, Toolbar } from "@extjs/ext-react";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { TableViewGridContainer } from "./TableViewGridContainer";
import { QualtityGateSetWindowContainer } from "./QualtityGateSetWindowContainer";
import { HistoryButtonView } from "../../History/Components/HistoryButtonView";
import { Model } from "../../Common/States/ExtStates";
import { TableViewRecord } from "../States/TableViewRecord";

export interface TableViewProps {
    buildHistotyDataLoadUrl(selection: Model<TableViewRecord>[], selectedDataIndex: string): string
}

export interface TableViewState {
    selection: Model<TableViewRecord>[]
    selectedDataIndex: string
    isEnableHistoryButton: boolean
}

export class TableView extends React.Component<TableViewProps, TableViewState> {
    constructor(props) {
        super(props)

        this.state = {
            selection: [],
            selectedDataIndex: null,
            isEnableHistoryButton: false
        };
    }

    public render() {
        const { selection, selectedDataIndex, isEnableHistoryButton } = this.state;
        const { buildHistotyDataLoadUrl } = this.props;

        return (
            <Container layout="fit">
                <Toolbar docked="top">
                    <HistoryButtonView  
                        isEnabled={isEnableHistoryButton}
                        flex={1}
                        buidHistoryUrl={() => buildHistotyDataLoadUrl(selection, selectedDataIndex)}
                    />
                    <QualtityGateSetWindowContainer/>
                </Toolbar>

                <TableViewGridContainer onSelectionChange={this.onSelectionChange} />
            </Container>
        );
    }

    protected onSelectionChange = (grid, records: Model<TableViewRecord>[], selecting: boolean, selectionInfo) => {
        // const { startCell } = selectionInfo;

        // if (startCell) {
        //     const column = selectionInfo.startCell.column;

        //     this.setState({
        //         selection: records,
        //         selectedDataIndex: column.getDataIndex(),
        //         isEnableHistoryButton: !!column.getEditable()
        //     });
        // } 
        // else {
        //     this.setState({
        //         selection: [],
        //         selectedDataIndex: null,
        //         isEnableHistoryButton: false
        //     });
        // }
    }
}