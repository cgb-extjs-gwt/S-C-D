import * as React from 'react';
import { Panel, Container } from '@extjs/ext-react';
import { ApprovalBundle } from '../States/ApprovalBundle';
import * as approvalService from '../Services/CostApprovalService';
import { ApprovalBundleItemComponent } from './ApprovalBundleItemComponent';
import { BundleFilter } from '../States/BundleFilter';

export interface ApprovalBundleListProps {
    filter?: BundleFilter
    flex?: number
}

export interface ApprovalBundleListState {
    filter: BundleFilter
    bundles: ApprovalBundle[]
    isReloadBundles: boolean
}

export class ApprovalBundleListComponent extends React.Component<ApprovalBundleListProps, ApprovalBundleListState> {
    constructor(props: ApprovalBundleListProps) {
        super(props);

        this.state = { 
            filter: props.filter,
            bundles: [],
            isReloadBundles: false
        };
    }

    componentWillReceiveProps(nextProps: ApprovalBundleListProps) {
        if (!this.equalsFilter(this.state.filter, nextProps.filter)) {
            this.setState({ filter: nextProps.filter });
            this.reloadBundles(nextProps.filter);
        }
    }

    shouldComponentUpdate(nextProps: ApprovalBundleListProps, nextState: ApprovalBundleListState) {
        return this.state.bundles !== nextState.bundles;
    }

    componentDidMount() {
        this.reloadBundles(this.state.filter);
    }

    render() {
        const { flex } = this.props;
        const { bundles, isReloadBundles } = this.state;

        return (
            <Container layout="vbox" flex={flex} scrollable>
                {
                    isReloadBundles
                        ? this.textContainer("Loading")
                        : bundles.length > 0
                            ? bundles.map(bundle => (
                                <ApprovalBundleItemComponent 
                                    key={bundle.id}
                                    bundle={bundle} 
                                    onHandled={() => this.reloadBundles(this.state.filter)}
                                />
                            ))
                            : this.textContainer("No items")
                }
            </Container>
        );
    }

    private reloadBundles = (filter: BundleFilter) => {
        this.setState({ 
            bundles: [],
            isReloadBundles: true 
        });

        approvalService.getBundles(filter)
                       .then(bundles => this.setState({ 
                           bundles,
                           isReloadBundles: false 
                        }));
    }

    private equalsFilter(filter1: BundleFilter, filter2: BundleFilter) {
        let result = true;

        if (filter1 !== filter2) {
            if (filter1 == null || filter2 == null) {
                result = false;
            } else if (filter1.dateTimeFrom !== filter2.dateTimeFrom ||
                filter1.dateTimeTo !== filter2.dateTimeTo ||
                !this.equalsArray(filter1.applicationIds, filter2.applicationIds) ||
                !this.equalsArray(filter1.costBlockIds, filter2.costBlockIds) ||
                !this.equalsArray(filter1.costElementIds, filter2.costElementIds) ||
                !this.equalsArray(filter1.userIds, filter2.userIds)) {
                result = false;
            }
        }

        return result;
    }

    private equalsArray<T>(array1: T[], array2: T[]) {
        let result: boolean;

        if (array1 === array2) {
            result = true;
        } else {
            if (array1.length !== array2.length) {
                result = false;
            } else {
                result = array1.every((value, index) => array1[index] === array2[index]);
            }
        }

        return result;
    }

    private textContainer(text: string) {
        return (
            <Container layout="center" padding="20">
                <h3>{ text }</h3>
            </Container>
        );
    }
}