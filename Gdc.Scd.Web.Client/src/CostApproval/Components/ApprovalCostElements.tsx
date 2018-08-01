import * as React from 'react';
import ApprovalBundles from './ApprovalBundles';
import FilterBundles from './FilterBundles';
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
                <FilterBundles />
            </Container>
        );
    }
}

export default ApprovalCostElements;