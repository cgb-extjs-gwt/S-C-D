import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { AutoGrid } from "./Components/AutoGrid";
import { AutoGridModel } from "./Model/AutogridModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

export class ReportView extends React.Component<any, AutoGridModel> {

    private id: string;

    private srv: IReportService;

    constructor(props: any) {

        var id = props.match.params.id;

        if (!id) {
            throw new Error('invalid report id');
        }

        super(props);
        this.init();

        this.id = id;
    }

    public componentDidMount() {
        this.srv.getSchema(this.id).then(x => this.setState(x));
    }

    public render() {

        let grid = null;

        if (this.state) {
            let schema = this.state;

            grid = (
                <AutoGrid columns={schema.fields} filter={schema.filter} url={this.getUrl()} title={schema.caption} />
            );
        }

        return grid;
    }

    private init() {
        this.srv = ReportFactory.getReportService();
    }

    public getUrl(): string {
        return buildMvcUrl('report', 'view', { 'id': this.id });
    }
}