import * as React from 'react';
import ApprovalBundles from './ApprovalBundles';
import { FilterBundleContainer } from './FilterBundlesContainer';
import { Container } from '@extjs/ext-react';


class ApprovalCostElements extends React.Component{
    render(){
        return (
            <Container
                layout={{
                    type: "hbox",
                    pack: "space-between"
                }}>
                <ApprovalBundles />
                <FilterBundleContainer />
            </Container>
        );
    }
}

export default ApprovalCostElements;