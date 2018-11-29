import * as React from "react";
import { TableViewGrid } from "../TableView/Components/TableViewGrid";
import { TableViewGridHelper } from "./Helpers/TableViewGridHelper";
import { ITableViewService } from "./Services/ITableViewService";
import { TableViewFactory } from "./Services/TableViewFactory";

export class TableView extends React.Component<any, any> {

    private srv: ITableViewService;

    public state = {

        schema: null,

        costMetaData: null

    }

    constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let grid = null;

        if (this.state) {

            let schema = this.state.schema;
            let meta = this.state.costMetaData;

            if (schema && meta) {

                let url = this.srv.getUrl();

                grid = <TableViewGrid
                    {...TableViewGridHelper.buildGridProps(url, schema, meta)}
                    store={null}// stub
                />;

            }
        }

        return grid;
    }

    public componentDidMount() {
        this.srv.getSchema().then(x => this.setState({ schema: x, costMetaData: null }));
    }

    private init() {
        this.srv = TableViewFactory.getTableViewService();
    }

}