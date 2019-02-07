import * as React from "react";
import { Container, FormPanel, Toolbar, Button } from "@extjs/ext-react";
import { Bundle } from "../States/ApprovalState";

export interface BundleListLayoutActions {
    onRefresh?()
}

export interface BundleListLayoutProps extends BundleListLayoutActions  {
    bundles: Bundle[]
}

export abstract class BundleListLayout extends React.Component<BundleListLayoutProps> {
    constructor(props: BundleListLayoutProps) {
        super(props);

        this.bundleItemRender = this.bundleItemRender.bind(this);
        this.filterRender = this.filterRender.bind(this);
    }

    public render() {
        const { bundles } = this.props;

        return (
            <Container layout={{type: "hbox", pack: "space-between"}}>
                <Container layout="vbox" flex={2} scrollable>
                    {
                        bundles && bundles.length > 0
                            ? bundles.map(this.bundleItemRender)
                            : <Container layout="center" padding="20">
                                <h3>No items</h3>
                            </Container>
                    }
                </Container>

                <FormPanel 
                    scrollable 
                    shadow
                    layout={{type: 'vbox', align: 'left'}}
                    flex={1}
                    title="Filter By"
                >
                    {this.filterRender()}
                    <Toolbar docked="bottom" layout={{type: 'vbox', align: 'center'}}>
                        <Button 
                            disabled={false} 
                            iconCls="x-fa fa-filter" 
                            text="Refresh" 
                            ui="action raised" 
                            handler={this.onRefresh}
                        />
                    </Toolbar>
                </FormPanel>
            </Container>
        );
    }

    protected abstract bundleItemRender(bundle: Bundle): any

    protected abstract filterRender(): any

    private onRefresh = () => {
        const { onRefresh } = this.props;

        onRefresh && onRefresh();
    }
}