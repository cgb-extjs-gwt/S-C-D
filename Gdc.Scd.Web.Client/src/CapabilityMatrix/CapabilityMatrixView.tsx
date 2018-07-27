import * as React from "react";
import { Container, Label, Button, Grid, Column, Toolbar } from "@extjs/ext-react";
import * as srv from "./fakes/FakeCapabilityMatrixServices";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";

export class CapabilityMatrixView extends React.Component<any, any> {

    private allowed: Grid;

    private denied: Grid;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="vbox">

                <Toolbar docked="top">
                    <Button iconCls="x-fa fa-edit" text="Edit" handler={this.onEditMatrix} />
                    <Button iconCls="x-fa fa-undo" text="Allow combinations" ui="confirm" handler={this.onAllow} />
                </Toolbar>

                <Grid ref="denied" store={this.state.denied} width="100%" minHeight="45%" title="Denied combinations" selectable="multi">
                    <Column flex="1" text="Country" dataIndex="country" />
                    <Column flex="1" text="WG(Asset)" dataIndex="WG" />
                    <Column flex="1" text="Availability" dataIndex="availability" />
                    <Column flex="1" text="Duration" dataIndex="duration" />
                    <Column flex="1" text="Reaction type" dataIndex="reactType" />
                    <Column flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <Column flex="1" text="Service location" dataIndex="serviceLocation" />
                    <Column flex="1" text="Fujitsu Global Portfolio" dataIndex="isGlobalPortfolio" />
                    <Column flex="1" text="Master Portfolio" dataIndex="isMasterPortfolio" />
                    <Column flex="1" text="Core Portfolio" dataIndex="isCorePortfolio" />
                </Grid>

                <Grid ref="allowed" store={this.state.allowed} width="100%" minHeight="45%" title="Allowed combinations">
                    <Column flex="1" text="Country" dataIndex="country" />
                    <Column flex="1" text="WG(Asset)" dataIndex="WG" />
                    <Column flex="1" text="Availability" dataIndex="availability" />
                    <Column flex="1" text="Duration" dataIndex="duration" />
                    <Column flex="1" text="Reaction type" dataIndex="reactType" />
                    <Column flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <Column flex="1" text="Service location" dataIndex="serviceLocation" />
                    <Column flex="1" text="Fujitsu Global Portfolio" dataIndex="isGlobalPortfolio" />
                    <Column flex="1" text="Master Portfolio" dataIndex="isMasterPortfolio" />
                    <Column flex="1" text="Core Portfolio" dataIndex="isCorePortfolio" />
                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        Promise.all([
            srv.getAllowed(),
            srv.getDenied()
        ]).then(x => {
            this.setState({
                allowed: x[0],
                denied: x[1]
            });
        });
        //
        this.allowed = this.refs['allowed'] as Grid;
        this.denied = this.refs['denied'] as Grid;
    }

    private init() {
        this.onEditMatrix = this.onEditMatrix.bind(this);
        this.onAllow = this.onAllow.bind(this);
        //
        this.state = {
            allowed: [],
            denied: []
        };
    }

    private onEditMatrix() {
        console.log('openEditPage()');
        //this.props.history.push('/capability-matrix/edit');
    }

    private onAllow() {
        let selected = this.getDenySelected();
        if (selected.length > 0) {
            ExtMsgHelper.confirm(
                'Allow combinations',
                'Do you want to remove denied combination?',
                () => this.allowCombination(selected)
            );
        }
    }

    private allowCombination(ids: string[]) {
        console.log('allowCombination()', ids);
    }

    public getDenySelected(): string[] {
        return ExtDataviewHelper.getGridSelected(this.denied, 'id');
    }
}