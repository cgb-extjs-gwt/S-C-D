export class WebworkerHelper {

    public static run(fn): Worker {
        return new Worker(URL.createObjectURL(new Blob(['(' + fn + ')()'])));
    }

}