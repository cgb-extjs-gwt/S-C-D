import { Button, Container, Grid, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { buildComponentUrl, buildMvcUrl } from "../Common/Services/Ajax";
import { FilterPanel } from "./Components/FilterPanel";
import { NullStringColumn } from "./Components/NullStringColumn";
import { ReadonlyCheckColumn } from "./Components/ReadonlyCheckColumn";
import { PortfolioFilterModel } from "./Model/PortfolioFilterModel";

export class PortfolioView extends React.Component<any, any> {

    private filter: FilterPanel;

    private allowStore: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 100,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('portfolio', 'allowed')
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

        let isMasterPortfolio = this.IsMasterPortfolio();

        return (
            <Container scrollable={true}>

                <FilterPanel ref="filter" docked="right" onSearch={this.onSearch} />

                <Toolbar docked="top">
                    <Button iconCls="x-fa fa-edit" text="Edit" handler={this.onEdit} />
                </Toolbar>

                <Grid
                    ref="allowed"
                    store={this.allowStore}
                    width="100%"
                    height="100%"
                    selectable={false}
                    plugins={['pagingtoolbar']}>

                    <NullStringColumn hidden={!isMasterPortfolio} flex="1" text="Country" dataIndex="country" />

                    <NullStringColumn flex="1" text="WG(Asset)" dataIndex="wg" />
                    <NullStringColumn flex="1" text="Availability" dataIndex="availability" />
                    <NullStringColumn flex="1" text="Duration" dataIndex="duration" />
                    <NullStringColumn flex="1" text="Reaction type" dataIndex="reactionType" />
                    <NullStringColumn flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <NullStringColumn flex="2" text="Service location" dataIndex="serviceLocation" />
                    <NullStringColumn flex="2" text="Pro active" dataIndex="proActive" />

                    <ReadonlyCheckColumn hidden={isMasterPortfolio} flex="1" text="Fujitsu principal portfolio" dataIndex="isGlobalPortfolio" />
                    <ReadonlyCheckColumn hidden={isMasterPortfolio} flex="1" text="Master portfolio" dataIndex="isMasterPortfolio" />
                    <ReadonlyCheckColumn hidden={isMasterPortfolio} flex="1" text="Core portfolio" dataIndex="isCorePortfolio" />
                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        this.filter = this.refs.filter as FilterPanel;
        //
        this.reload();
    }

    private init() {
        this.onEdit = this.onEdit.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.allowStore.on('beforeload', this.onBeforeLoad, this);
    }

    private onEdit() {
        this.props.history.push(buildComponentUrl('/portfolio/edit'));
    }

    private onSearch(filter: PortfolioFilterModel) {
        this.reload();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }

    private reload() {
        this.allowStore.load();

        this.setState({ ___: new Date().getTime() }); //stub, re-paint ext grid
    }

    private IsMasterPortfolio(): boolean {
        let result = false;
        if (this.filter) {
            let filter = this.filter.getModel();
            result = !!(filter && filter.country);
        }
        return result;
    }
}