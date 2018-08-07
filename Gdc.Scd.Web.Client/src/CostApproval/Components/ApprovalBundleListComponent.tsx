import * as React from 'react';
import { Panel, Container } from '@extjs/ext-react';
import { ApprovalBundle } from '../States/ApprovalBundle';
import { BuldleFilter } from '../States/BuldleFilter';
import * as approvalService from '../Services/CostApprovalService';
import { ApprovalBundleItemComponent } from './ApprovalBundleItemComponent';

export interface ApprovalBundleListProps {
    filter?: BuldleFilter
    flex?: number
}

export interface ApprovalBundleListState {
    filter: BuldleFilter
    bundles: ApprovalBundle[]
}

export class ApprovalBundleListComponent extends React.Component<ApprovalBundleListProps, ApprovalBundleListState> {
    constructor(props: ApprovalBundleListProps) {
        super(props);

        this.state = { 
            filter: props.filter,
            bundles: []
        };
    }

    componentDidMount() {
        this.reloadBundles();
    }

    render() {
        const { flex } = this.props;
        const { bundles } = this.state;

        return (
            <Container layout="vbox" flex={flex} scrollable>
                {
                    bundles.length > 0
                        ? bundles.map(bundle => (
                            <ApprovalBundleItemComponent key={bundle.id} bundle={bundle} onHandled={this.reloadBundles}/>
                        ))
                        : <Container layout="center" padding="20">
                            <h3>No items</h3>
                          </Container>
                }
            </Container>
        );
    }

    private reloadBundles = () => {
        this.setState({ bundles: [] });

        approvalService.getBundles(this.state.filter)
                       .then(bundles => this.setState({ bundles }));
    }
}