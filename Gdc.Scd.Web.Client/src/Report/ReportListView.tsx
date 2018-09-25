import { Column, Container, Grid } from "@extjs/ext-react";
import * as React from "react";
import { ReadonlyCheckColumn } from "../CapabilityMatrix/Components/ReadonlyCheckColumn";
import { buildComponentUrl, buildMvcUrl } from "../Common/Services/Ajax";

export class ReportListView extends React.Component<any, any> {

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        pageSize: 50,

        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('report', 'getall')
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
            <Container layout="fit">

                <Grid
                    ref="grid"
                    store={this.store}
                    width="100%"
                    title="SCD reports"
                    plugins={['pagingtoolbar']}>

                    <Column flex="1" text="Name" dataIndex="name" />
                    <Column flex="1" text="Title" dataIndex="title" />
                    <ReadonlyCheckColumn flex="1" text="Country specific" dataIndex="CountrySpecific" />
                    <ReadonlyCheckColumn flex="1" text="Has freesed version" dataIndex="HasFreesedVersion" />

                </Grid>
            </Container>
        );
    }

    private init() {
        this.onOpenLink = this.onOpenLink.bind(this);
    }

    private onOpenLink(e) {

        let target = e.target as HTMLElement;
        let href = target.getAttribute('data-href');

        if (href) {
            href = buildComponentUrl(href);
            this.props.history.push(href);
        }
    }

}