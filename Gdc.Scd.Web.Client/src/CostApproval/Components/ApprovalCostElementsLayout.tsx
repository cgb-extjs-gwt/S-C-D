import * as React from 'react';
import { FilterBundleContainer } from './FilterBundlesContainer';
import { Container } from '@extjs/ext-react';
import { ApprovalBundleListContainerComponent } from './ApprovalBundleListContainerComponent';

export class ApprovalCostElementsLayout extends React.Component{
    render(){
        return (
            <Container
                layout={{
                    type: "hbox",
                    pack: "space-between"
                }}>
                <ApprovalBundleListContainerComponent flex={2}/>
                <FilterBundleContainer/>
            </Container>
        );
    }
}

export default ApprovalCostElementsLayout;