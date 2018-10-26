import { Container, ContainerProps } from "@extjs/ext-react";
import * as React from "react";
import { RemoteNotify } from "../../Webworker/RemoteNotify";
import { RemoteAction } from "../Actions/NotifyActions";

export class AlertPanel extends React.Component<ContainerProps, any> {

    state = {
        items: new Array<RemoteAction>()
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

    private createAlert(model: RemoteAction, index: number): JSX.Element {

        let css = 'alert info'; //todo: add switch by alert type

        return (
            <div key={index} className={css}>
                <span data-id={index} className="alert-close">&times;</span>
                <strong>Info!</strong> {model.text}
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

    private onAlert(data: RemoteAction) {
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