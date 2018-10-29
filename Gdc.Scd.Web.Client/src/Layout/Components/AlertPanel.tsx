import { Container, ContainerProps } from "@extjs/ext-react";
import * as React from "react";
import { RemoteNotify } from "../../Webworker/RemoteNotify";
import { AlertAction, APP_ALERT_ERROR, APP_ALERT_INFO, APP_ALERT_LINK, APP_ALERT_REPORT, APP_ALERT_SUCCESS, APP_ALERT_WARNING, LinkAction, ReportAction } from "../Actions/AlertActions";

export class AlertPanel extends React.Component<ContainerProps, any> {

    state = {
        items: new Array<AlertAction>()
    }

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        let items = this.state.items;
        let hideAll = null;

        if (items.length > 1) {
            hideAll = <a className="alert-close-all">Hide all</a>;
        }

        return (
            <Container {...this.props} margin="0 0 5px 0" padding="20px" bottom="10px" right="10px" maxWidth="450px">
                <div onClick={this.onCloseAlert}>
                    {items.map(this.createAlert)}
                    {hideAll}
                </div>
            </Container>
        );
    }

    private createAlert(model: AlertAction, index: number): JSX.Element {
        switch (model.type) {

            case APP_ALERT_ERROR:
                return this.textAlert('alert', 'Error!', model.text, index);

            case APP_ALERT_SUCCESS:
                return this.textAlert('alert success', 'Success!', model.text, index);


            case APP_ALERT_WARNING:
                return this.textAlert('alert warning', 'Warning!', model.text, index);

            case APP_ALERT_REPORT:
                return this.autoloadAlert(model as ReportAction, index);

            case APP_ALERT_LINK:
                return this.linkAlert(model as LinkAction, index);

            case APP_ALERT_INFO:
            default:
                return this.textAlert('alert info', 'Info!', model.text, index);
        }
    }

    private textAlert(css: string, caption: string, text: string, index: number): JSX.Element {
        return (
            <div key={index} className={css}>
                <span data-id={index} className="alert-close">&times;</span>
                <strong>{caption}</strong> {text}
            </div>
        );
    }

    private autoloadAlert(model: ReportAction, index: number): JSX.Element {
        return (
            <div key={index} className="alert success">
                <span data-id={index} className="alert-close">&times;</span>
                <strong>Success!</strong> {model.text}
                <iframe className="alert-frame" src={model.url}></iframe>
            </div>
        );
    }

    private linkAlert(model: LinkAction, index: number): JSX.Element {
        return (
            <div key={index} className="alert success">
                <span data-id={index} className="alert-close">&times;</span>
                <strong>Success!</strong> <a href={model.url} target="_blank">{model.text} download...</a>
            </div>
        );
    }

    private init() {
        this.createAlert = this.createAlert.bind(this);
        this.onAlert = this.onAlert.bind(this);
        this.onCloseAlert = this.onCloseAlert.bind(this);
        //
        RemoteNotify(this.onAlert);
    }

    private onAlert(data: AlertAction) {
        this.setState({ items: [...this.state.items, data] });
    }

    private onCloseAlert(e: any): void {
        let target = e.target;
        let el = target.className;
        let array;

        if (el === 'alert-close') {
            let index = target.getAttribute('data-id');
            array = [...this.state.items];
            array.splice(index, 1);
        }
        else if (el === 'alert-close-all') {
            array = [];
        }
        else {
            return;
        }

        this.setState({ items: array });
    }
}