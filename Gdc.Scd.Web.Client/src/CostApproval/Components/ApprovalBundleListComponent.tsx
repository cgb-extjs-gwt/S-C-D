import * as React from 'react';
import { Panel, Container } from '@extjs/ext-react';
import { ApprovalBundle } from '../States/ApprovalBundle';
import * as approvalService from '../Services/CostApprovalService';
import { ApprovalBundleItemComponent } from './ApprovalBundleItemComponent';
import { BundleFilter } from '../States/BundleFilter';
import { ApprovalBundleState } from '../States/ApprovalBundleState';

export interface ApprovalBundleListActions {
    reloadBundles?()
}

export interface ApprovalBundleListProps extends ApprovalBundleListActions {
    bundles?: ApprovalBundle[]
    flex?: number
    buildChildrenBundleItem?(bundle: ApprovalBundle, onHandled: () => void): any
}

export class ApprovalBundleListComponent extends React.Component<ApprovalBundleListProps> {
    shouldComponentUpdate(nextProps: ApprovalBundleListProps) {
        return this.props.bundles !== nextProps.bundles;
    }

    render() {
        const { flex, buildChildrenBundleItem, bundles, reloadBundles } = this.props;

        return (
            <Container layout="vbox" flex={flex} scrollable>
                {
                    bundles && bundles.length > 0
                        ? bundles.map(bundle => (
                            <ApprovalBundleItemComponent 
                                key={bundle.id}
                                bundle={bundle} 
                            >
                                { 
                                    buildChildrenBundleItem && 
                                    buildChildrenBundleItem(bundle, reloadBundles) 
                                }
                            </ApprovalBundleItemComponent>
                        ))
                        : this.textContainer("No items")
                }
            </Container>
        );
    }

    private textContainer(text: string) {
        return (
            <Container layout="center" padding="20">
                <h3>{ text }</h3>
            </Container>
        );
    }
}