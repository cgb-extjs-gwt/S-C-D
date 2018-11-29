import * as React from "react";
import { CostMetaData } from "../Common/States/CostMetaStates";
import { TableViewGrid } from "./Components/TableViewGrid";
import { TableViewInfo } from "../TableView/States/TableViewState";
import { TableViewGridHelper } from "./Helpers/TableViewGridHelper";
import { ITableViewService } from "./Services/ITableViewService";
import { TableViewFactory } from "./Services/TableViewFactory";
import { StoreOperation } from "../Common/States/ExtStates";

export interface TableViewState {
    meta: CostMetaData;
    schema: TableViewInfo
}

export class TableView extends React.Component<any, TableViewState> {

    private grid: TableViewGrid;

    private srv: ITableViewService;

    private editedRecords: [];

    public state: TableViewState;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let grid = null;

        if (this.state) {

            let meta = this.state.meta;
            let schema = this.state.schema;
            let url = this.srv.getUrl();

            if (meta && schema) {
                grid = <TableViewGrid
                    {...TableViewGridHelper.buildGridProps(url, schema, meta)}
                    ref={x => this.grid = x}
                    onApprove={this.onApprove}
                    onUpdateRecord={this.onUpdateRecord}
                    onUpdateRecordSet={this.onUpdateRecordSet}
                    onCancel={this.onCancel}
                />;
            }
        }

        return grid;
    }

    public componentDidMount() {
        Promise.all([
            this.srv.getMeta(),
            this.srv.getSchema()
        ]).then(x => this.setState({
            meta: x[0],
            schema: x[1]
        }));
    }

    private init() {
        this.srv = TableViewFactory.getTableViewService();
        //
        this.onApprove = this.onApprove.bind(this);
        this.onUpdateRecord = this.onUpdateRecord.bind(this);
        this.onUpdateRecordSet = this.onUpdateRecordSet.bind(this);
        this.onCancel = this.onCancel.bind(this);
    }

    private onApprove() {
        console.log('onApprove()');
    }

    private onUpdateRecord(store, record, operation, modifiedFieldNames) {
        if (operation === StoreOperation.Edit) {
            const [dataIndex] = modifiedFieldNames;
            const countDataIndex = TableViewGridHelper.buildCountDataIndex(dataIndex);

            if (record.get(countDataIndex) == 0) {
                record.data[countDataIndex] = 1;
            }
        }
    }

    private onUpdateRecordSet(records, operation, dataIndex) {
        if (operation === StoreOperation.Edit) {
            const tableViewRecords = records.map(rec => rec.data);

            this.editRecord(tableViewRecords, dataIndex);
        }
    }

    private onCancel() {
        this.resetChanges();
    }

    private editRecord (state, action) {
        //let editedRecords = this.editedRecords;

        //action.records.forEach(actionRecord => {
        //    const recordIndex = editedRecords.findIndex(editRecord => TableViewGridHelper.isEqualCoordinates(editRecord, actionRecord));

        //    const changedData = {
        //        [action.dataIndex]: actionRecord.data[action.dataIndex]
        //    };

        //    if (recordIndex == -1) {
        //        editedRecords = [
        //            ...editedRecords,
        //            {
        //                coordinates: actionRecord.coordinates,
        //                data: changedData,
        //                additionalData: actionRecord.additionalData
        //            }
        //        ];
        //    }
        //    else {
        //        editedRecords = editedRecords.map(
        //            (record, index) =>
        //                index == recordIndex
        //                    ? {
        //                        coordinates: actionRecord.coordinates,
        //                        data: {
        //                            ...record.data,
        //                            ...changedData
        //                        },
        //                        additionalData: actionRecord.additionalData
        //                    }
        //                    : record
        //        );
        //    }
        //});

        //this.editedRecords = editedRecords;
    }

    private resetChanges() {
        this.editedRecords = [];
    }

}