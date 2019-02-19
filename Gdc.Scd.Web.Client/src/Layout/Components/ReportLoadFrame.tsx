import * as React from "react";

export interface ReportLoadFrameProps {
    url: string;
    report: string;
    filter: any;
}

export class ReportLoadFrame extends React.Component<ReportLoadFrameProps, any>{

    private token: string;

    constructor(props: ReportLoadFrameProps) {
        super(props);
        this.token = 'frm_' + new Date().getTime();
    }

    public render() {
        return <iframe src="about:blank" id={this.token} name={this.token}></iframe>;
    }

    public componentDidMount() {
        var me = this as any;

        var form = document.createElement("form");
        form.setAttribute("target", this.token);
        form.setAttribute("method", "post");
        form.setAttribute("action", this.props.url);

        this.addFormData(form, 'report', this.props.report);
        this.addFormData(form, 'filter', JSON.stringify(this.props.filter))

        me.getDOMNode().appendChild(form);
        form.submit();
    }

    private addFormData(form, name, value) {
        var hdf = document.createElement("input");
        //
        hdf.setAttribute('type', 'hidden');
        hdf.setAttribute('name', name);
        hdf.setAttribute('value', value);
        //
        form.appendChild(hdf);
    }
}