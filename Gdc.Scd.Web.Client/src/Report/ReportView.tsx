import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { AutoGrid } from "./Components/AutoGrid";
import { AutoGridModel } from "./Model/AutogridModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

export class ReportView<T=any> extends React.Component<T, AutoGridModel> {

    private name: string;

    private srv: IReportService;

    constructor(props: any) {

        var rname = props.match.params.name;

        if (!rname) {
            throw new Error('invalid report name');
        }

        super(props);
        this.init();

        this.name = rname;
    }

    public componentDidMount() {
        this.srv.getSchemaByName(this.name).then(x => this.setState(x));
    }

    public render() {

        let grid = null;

        if (this.state) {
            let schema = this.state;

            grid = <AutoGrid columns={schema.fields} filter={schema.filter} url={this.getUrl()} downloadUrl={this.getDownloadUrl()} title={schema.title} />;
        }

        return grid;
    }

    private init() {
        this.srv = ReportFactory.getReportService();
    }

    public getUrl(): string {
        return buildMvcUrl('report', 'view', { 'id': this.state.id });
    }

    public getDownloadUrl(): string {
        return buildMvcUrl('report', 'export', { 'id': this.state.id });
    }
}