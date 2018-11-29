import * as React from "react";
import { CostMetaData } from "../Common/States/CostMetaStates";
import { TableViewGrid } from "../TableView/Components/TableViewGrid";
import { TableViewInfo } from "../TableView/States/TableViewState";
import { TableViewGridHelper } from "./Helpers/TableViewGridHelper";
import { ITableViewService } from "./Services/ITableViewService";
import { TableViewFactory } from "./Services/TableViewFactory";

export interface TableViewState {
    meta: CostMetaData;
    schema: TableViewInfo
}

export class TableView extends React.Component<any, TableViewState> {

    private srv: ITableViewService;

    public state: TableViewState;

    constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let grid = null;

        if (this.state) {

            let meta = this.state.meta;
            let schema = this.state.schema;
            let url = this.srv.getUrl();

            console.log('schema:', schema);
            console.log('meta:', meta);

            if (meta && schema) {
                grid = <TableViewGrid {...TableViewGridHelper.buildGridProps(url, schema, meta)} />;
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
    }

}