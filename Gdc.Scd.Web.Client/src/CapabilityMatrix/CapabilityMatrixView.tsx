import { Button, Container, Grid, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { FilterPanel } from "./Components/FilterPanel";
import { NullStringColumn } from "./Components/NullStringColumn";
import { ReadonlyCheckColumn } from "./Components/ReadonlyCheckColumn";
import { CapabilityMatrixFilterModel } from "./Model/CapabilityMatrixFilterModel";
import { ICapabilityMatrixService } from "./Services/ICapabilityMatrixService";
import { MatrixFactory } from "./Services/MatrixFactory";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { buildComponentUrl } from "../Common/Services/Ajax";

export class CapabilityMatrixView extends React.Component<any, any> {

    private allowed: Grid;

    private denied: Grid;

    private filter: FilterPanel;

    private srv: ICapabilityMatrixService;

    private allowStore: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('capabilitymatrix','allowed')
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        }
    });

    private denyStore: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('capabilitymatrix','denied')
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        }
    });

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

                <Grid
                    ref="denied"
                    store={this.denyStore}
                    width="100%"
                    height="45%"
                    minHeight="45%"
                    title="Denied combinations"
                    selectable="multi"
                    plugins={['pagingtoolbar']}>
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

                <Grid
                    ref="allowed"
                    store={this.allowStore}
                    width="100%"
                    height="45%"
                    minHeight="45%"
                    title="Allowed combinations"
                    selectable={false}
                    plugins={['pagingtoolbar']}>
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
        this.onBeforeLoad = this.onBeforeLoad.bind(this);
        //
        this.allowStore.on('beforeload', this.onBeforeLoad);
        this.denyStore.on('beforeload', this.onBeforeLoad);
    }

    private onEdit() {
        this.props.history.push(buildComponentUrl('/capability-matrix/edit'));
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
        this.denyStore.load();
        this.allowStore.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }
}