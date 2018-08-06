import * as React from "react";
import { Container, Button, Grid, Toolbar } from "@extjs/ext-react";
import { ICapabilityMatrixService } from "./Services/ICapabilityMatrixService";
import { MatrixFactory } from "./Services/MatrixFactory";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { NullStringColumn } from "./Components/NullStringColumn";
import { ReadonlyCheckColumn } from "./Components/ReadonlyCheckColumn";
import { FilterPanel } from "./Components/FilterPanel";
import { CapabilityMatrixFilterModel } from "./Model/CapabilityMatrixFilterModel";

export class CapabilityMatrixView extends React.Component<any, any> {

    private allowed: Grid;

    private denied: Grid;

    private filter: FilterPanel;

    private srv: ICapabilityMatrixService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Container scrollable={true}>

                <FilterPanel ref="filter" docked="right" onSearch={this.onSearch} />

                <Toolbar docked="top">
                    <Button iconCls="x-fa fa-edit" text="Edit" handler={this.onEdit} />
                    <Button iconCls="x-fa fa-undo" text="Allow combinations" ui="confirm" handler={this.onAllow} />
                </Toolbar>

                <Grid ref="denied" store={this.state.denied} width="100%" minHeight="45%" title="Denied combinations" selectable="multi" plugins={['pagingtoolbar']}>
                    <NullStringColumn flex="1" text="Country" dataIndex="country" />
                    <NullStringColumn flex="1" text="WG(Asset)" dataIndex="wg" />
                    <NullStringColumn flex="1" text="Availability" dataIndex="availability" />
                    <NullStringColumn flex="1" text="Duration" dataIndex="duration" />
                    <NullStringColumn flex="1" text="Reaction type" dataIndex="reactionType" />
                    <NullStringColumn flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <NullStringColumn flex="1" text="Service location" dataIndex="serviceLocation" />
                    <ReadonlyCheckColumn flex="1" text="Fujitsu global portfolio" dataIndex="isGlobalPortfolio" />
                    <ReadonlyCheckColumn flex="1" text="Master portfolio" dataIndex="isMasterPortfolio" />
                    <ReadonlyCheckColumn flex="1" text="Core portfolio" dataIndex="isCorePortfolio" />
                </Grid>

                <Grid ref="allowed" store={this.state.allowed} width="100%" minHeight="45%" title="Allowed combinations" selectable={false} plugins={['pagingtoolbar']}>
                    <NullStringColumn flex="1" text="Country" dataIndex="country" />
                    <NullStringColumn flex="1" text="WG(Asset)" dataIndex="wg" />
                    <NullStringColumn flex="1" text="Availability" dataIndex="availability" />
                    <NullStringColumn flex="1" text="Duration" dataIndex="duration" />
                    <NullStringColumn flex="1" text="Reaction type" dataIndex="reactionType" />
                    <NullStringColumn flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <NullStringColumn flex="1" text="Service location" dataIndex="serviceLocation" />
                    <ReadonlyCheckColumn flex="1" text="Fujitsu global portfolio" dataIndex="isGlobalPortfolio" />
                    <ReadonlyCheckColumn flex="1" text="Master portfolio" dataIndex="isMasterPortfolio" />
                    <ReadonlyCheckColumn flex="1" text="Core portfolio" dataIndex="isCorePortfolio" />
                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        this.allowed = this.refs.allowed as Grid;
        this.denied = this.refs.denied as Grid;
        this.filter = this.refs.filter as FilterPanel;
        //
        this.reload();
    }

    private init() {
        this.srv = MatrixFactory.getMatrixService();
        this.onEdit = this.onEdit.bind(this);
        this.onAllow = this.onAllow.bind(this);
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            allowed: [],
            denied: []
        };
    }

    private onEdit() {
        this.props.history.push('/capability-matrix/edit');
    }

    private onAllow() {
        let selected = this.getDenySelected();
        if (selected.length > 0) {
            ExtMsgHelper.confirm(
                'Allow combinations',
                'Do you want to remove denied combination(s)?',
                () => this.allowCombination(selected)
            );
        }
    }

    private onSearch(filter: CapabilityMatrixFilterModel) {
        this.reload();
    }

    private allowCombination(ids: string[]) {
        this.srv.allowItems(ids).then(x => this.reload());
    }

    private getDenySelected(): string[] {
        return ExtDataviewHelper.getGridSelected(this.denied, 'id');
    }

    private reload() {
        let filter = this.filter.getModel();

        this.srv.getAllowed(filter).then(x => this.setState({ allowed: x.items }));
        this.srv.getDenied(filter).then(x => this.setState(
            {
                denied: {
                    data: x.items,
                    pageSize: 10
                }
            }));
    }
}