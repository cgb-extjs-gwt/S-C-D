import * as React from "react";
import { Container, Label, Button, Grid, Column } from "@extjs/ext-react";
import * as srv from "./fakes/FakeCapabilityMatrixServices";

export class CapabilityMatrixView extends React.Component<any, any> {

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="vbox">

                <Container>
                    <Button text="Edit" handler={this.onEditMatrix} />
                </Container>

                <Grid store={this.state.allowed} width="100%" minHeight="350" title="Allowed combinations" defaults="{{ 'flex': 1 }}">
                    <Column text="Country" dataIndex="country" />
                    <Column text="WG(Asset)" dataIndex="WG" />
                    <Column text="Availability" dataIndex="availability" />
                    <Column text="Duration" dataIndex="duration" />
                    <Column text="Reaction type" dataIndex="reactType" />
                    <Column text="Reaction time" dataIndex="reactionTime" />
                    <Column text="Service location" dataIndex="serviceLocation" />
                    <Column text="Fujitsu Global Portfolio" dataIndex="isGlobalPortfolio" />
                    <Column text="Master Portfolio" dataIndex="isMasterPortfolio" />
                    <Column text="Core Portfolio" dataIndex="isCorePortfolio" />
                </Grid>

                <Grid store={this.state.denied} width="100%" minHeight="350" title="Denied combinations">
                    <Column text="Country" dataIndex="country" />
                    <Column text="WG(Asset)" dataIndex="WG" />
                    <Column text="Availability" dataIndex="availability" />
                    <Column text="Duration" dataIndex="duration" />
                    <Column text="Reaction type" dataIndex="reactType" />
                    <Column text="Reaction time" dataIndex="reactionTime" />
                    <Column text="Service location" dataIndex="serviceLocation" />
                    <Column text="Fujitsu Global Portfolio" dataIndex="isGlobalPortfolio" />
                    <Column text="Master Portfolio" dataIndex="isMasterPortfolio" />
                    <Column text="Core Portfolio" dataIndex="isCorePortfolio" />
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
    }

    private init() {
        this.onEditMatrix = this.onEditMatrix.bind(this);
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
}