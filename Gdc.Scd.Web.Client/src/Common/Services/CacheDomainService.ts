import { DomainService } from "./DomainService";

export class CacheDomainService<T> {

    private controllerName: string;

    private data: T[];

    public constructor(cname: string) {
        this.controllerName = cname;
    }

    public getAll(): Promise<T[]> {
        if (this.data) {
            return Promise.resolve(this.data);
        }
        else {
            return new DomainService<T>(this.controllerName)
                .getAll()
                .then(x => {
                    this.data = x;
                    return this.data;
                });
        }
    }

}