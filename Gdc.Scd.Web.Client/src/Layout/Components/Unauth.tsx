import * as React from "react";
import { Container } from "@extjs/ext-react";

const Unauth = (props) => {
    return (
        <Container margin="10 0 0 10">
            <h1>NOT AUTHORIZED</h1>
            <div>You do not have permission to view this directory or page using
                 the credentials that you supplied</div>
            <hr />
            <div>Please contact <a href="mailto:SCD_Admin@ts.fujitsu.com">administrators</a> for assistance.</div>
        </Container>
    );
}

export default Unauth;