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

    // shouldComponentUpdate(newProps: ApprovalBundleListProps) {
    //     let result = false;

    //     const stateFilter = this.state.filter;
    //     const propsFilter = newProps.filter;

    //     if (stateFilter.dateTimeFrom !== propsFilter.dateTimeFrom ||
    //         stateFilter.dateTimeTo !== propsFilter.dateTimeTo ||
    //         !this.equalsArray(stateFilter.applicationIds, propsFilter.applicationIds) ||
    //         !this.equalsArray(stateFilter.costBlockIds, propsFilter.costBlockIds) ||
    //         !this.equalsArray(stateFilter.costElementIds, propsFilter.costElementIds) ||
    //         !this.equalsArray(stateFilter.userIds, propsFilter.userIds)) {
    //         result = true;
    //     } else {
    //         this.setState({ filter: propsFilter });
    //         this.reloadBundles();
    //     }

    //     return result;
    // }

    componentDidMount() {
        this.reloadBundles();
    }

    render() {
        const { flex } = this.props;

        return (
            <Container layout="vbox" flex={flex} scrollable>
                {
                    this.state.bundles.map(bundle => (
                        <ApprovalBundleItemComponent key={bundle.id} bundle={bundle} onHandled={this.reloadBundles}/>
                    ))
                }
            </Container>
        );
    }

    // private equalsArray<T>(array1: T[], array2: T[]) {
    //     let result: boolean;

    //     if (array1 === array2) {
    //         result = true;
    //     } else {
    //         if (array1.length !== array2.length) {
    //             result = false;
    //         } else {
    //             result = array1.every((value, index) => array1[index] === array2[index]);
    //         }
    //     }

    //     return result;
    // }

    private reloadBundles() {
        this.setState({ bundles: [] });

        approvalService.getBundles(this.state.filter)
                       .then(bundles => this.setState({ bundles }));
    }
}