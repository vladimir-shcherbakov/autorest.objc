
# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# IMPORTANT NOTE

This project uses a git submodule for dependent code. When cloning this repository use `git clone --recursive ...` or perform a `git submodule init ` after the project is cloned.



# AutoRest extension configuration

``` yaml
use-extension:
  "@microsoft.azure/autorest.modeler": "2.3.45" # keep in sync with package.json's dev dependency in order to have meaningful tests

pipeline:
  objc/imodeler1:
    input: openapi-document/identity
    output-artifact: code-model-v1
    scope: objc
  objc/commonmarker:
    input: imodeler1
    output-artifact: code-model-v1
  objc/cm/transform:
    input: commonmarker
    output-artifact: code-model-v1
  objc/cm/emitter:
    input: transform
    scope: scope-cm/emitter
  objc/generate:
    plugin: objc
    input: cm/transform
    output-artifact: source-file-objc
  objc/transform:
    input: generate
    output-artifact: source-file-objc
    scope: scope-transform-string
  objc/emitter:
    input: transform
    scope: scope-objc/emitter

scope-objc/emitter:
  input-artifact: source-file-objc
  output-uri-expr: $key

output-artifact:
- source-file-objc
```
