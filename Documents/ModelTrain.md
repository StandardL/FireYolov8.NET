# <center>模型训练</center>
## 1. 准备数据集
- 数据集存放路径为./ultraytics-main/datasets/FireDataSet，其中有三个文件夹，test、train、vaild分别对应的是测试集、训练集、验证集，每个文件夹下有两个文件夹，images和labels，分别对应图片及其对应的标签位置信息。
- 数据集参数配置文件的路径为./ultraytics/datasets/FireDataSet/data.yaml，其中包含了训练集、验证集、测试集的路径，可以根据需要进行修改。
## 2. 参数调整
- 参数调整代码存放路径为./ultraytics-main/cfg/default.yaml，其中包含了训练所需的所有参数，包括默认训练参数，详细训练参数、超参数、导出模型参数等等，可以根据需要进行修改。

## 3.训练
- 训练代码存放路径为./ultraytics-main/fire_train.py，训练完成后会在./ultraytics-main/runs/detect/train/weight下生成权重文件，用于后续的预测。

## 4.模型保存
- 如果想要另外保存其他类型的模型，可以在./ultraytics-main/fire_train.py中的export()函数中添加代码
- 例如,想要导出格式为onnx的预训练模型：model.export(format = 'onnx')
