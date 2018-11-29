import * as React from "react";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { CostMetaData } from "../Common/States/CostMetaStates";
import { StoreOperation } from "../Common/States/ExtStates";
import { TableViewRecord } from "../TableView/States/TableViewRecord";
import { TableViewInfo } from "../TableView/States/TableViewState";
import { TableViewGrid } from "./Components/TableViewGrid";
import { TableViewGridHelper } from "./Helpers/TableViewGridHelper";
import { ITableViewService } from "./Services/ITableViewService";
import { TableViewFactory } from "./Services/TableViewFactory";

export interface TableViewState {
    meta: CostMetaData;
    schema: TableViewInfo
}

export class TableView extends React.Component<any, TableViewState> {

    private grid: TableViewGrid;

    private srv: ITableViewService;

    private editedRecords: Array<TableViewRecord>;

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
                    onCancel={this.onCancel}
                    onSave={this.onSave}
                    onUpdateRecord={this.onUpdateRecord}
                    onUpdateRecordSet={this.onUpdateRecordSet}
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
        this.reset();
        this.onApprove = this.onApprove.bind(this);
        this.onCancel = this.onCancel.bind(this);
        this.onSave = this.onSave.bind(this);
        this.onUpdateRecord = this.onUpdateRecord.bind(this);
        this.onUpdateRecordSet = this.onUpdateRecordSet.bind(this);
    }

    private onApprove() {
        this.save(true);
    }

    private onCancel() {
        this.reset();
    }

    private onSave() {
        this.save(false);
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

    private save(isApproving: boolean) {
        let p = this.srv.updateRecords(this.editedRecords, { isApproving: isApproving })
            .then(x => {

                if (x.hasErrors) {
                    //dispatch(loadQualityCheckResult(x));
                }
                else {
                    //dispatch(resetQualityCheckResult());
                    //dispatch(resetChanges());
                }
            })

        handleRequest(p);
    }

    private editRecord(records: TableViewRecord[], index: number) {
        let recs = this.editedRecords;

        records.forEach(actionRecord => {
            const recordIndex = recs.findIndex(editRecord => TableViewGridHelper.isEqualCoordinates(editRecord, actionRecord));

            const changedData = {
                [index]: actionRecord.data[index]
            };

            if (recordIndex == -1) {
                recs = [
                    ...recs,
                    {
                        coordinates: actionRecord.coordinates,
                        data: changedData,
                        additionalData: actionRecord.additionalData
                    }
                ];
            }
            else {
                recs = recs.map(
                    (record, index) =>
                        index == recordIndex
                            ? {
                                coordinates: actionRecord.coordinates,
                                data: {
                                    ...record.data,
                                    ...changedData
                                },
                                additionalData: actionRecord.additionalData
                            }
                            : record
                );
            }
        });

        this.editedRecords = recs;
    }

    private reset() {
        this.editedRecords = [];
    }
}