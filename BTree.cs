using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace B_Tree
{
    public class BTree
    {
        private Node _root;

        private readonly int _size;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="degree">Порядок дерева</param>
        /// <exception cref="ArgumentException">Порядок не может быть меньше 2</exception>
        public BTree(int degree)
        {
            if (degree < 2)
            {
                throw new ArgumentException("BTree degree must be at least 2", "degree");
            }

            _root = new Node(degree);
            _size = degree;
        }

        #region Search

        /// <summary>
        /// Метод, который используется для поиска значений
        /// </summary>
        /// <param name="key">значение для поиска</param>
        /// <returns>Вывод пути к значения в формате 4 => 7 => 11</returns>
        public string Search(int key)
        {
            return SearchInternal(_root, key);
        }

        /// <summary>
        /// Так же метод для поиска, но приватный
        /// </summary>
        /// <param name="node">нода, в которой поиск ведётся</param>
        /// <param name="value">значение, которое ищем</param>
        /// <param name="pathToValue">сам путь</param>
        /// <returns>путь</returns>
        private string SearchInternal(Node node, int value, string pathToValue = "")
        {
            int index = 0;
            //Проходимся по текущей ноде и ищем значение
            for (int i = 0; i < node.Values.Count; i++)
            {
                if (value > node.Values[i])
                {
                    index++;
                }
            }
            //Если нашли то возвращаем путь
            if (index < node.Values.Count && node.Values[index] == value)
            {
                return pathToValue + node.Values[index];
            }

            //Если нет, вписывваем значение и идём дальше(если значения нет, то выводи ошибку)
            var tempindex = index > 0 ? index - 1 : 0;
            pathToValue += $"{node.Values[tempindex]} => ";

            return node.IsLeaf ? "Такого значения не существует в дереве" : SearchInternal(node.Leaves[index], value, pathToValue);
        }

        #endregion

        #region Insert
        /// <summary>
        /// Метод для вставки значений
        /// </summary>
        /// <param name="newValueToInsert">Значение для вставки</param>
        public void Insert(int newValueToInsert)
        {
            //Проверяем заполнен ли рут
            if (!_root.IsFull)
            {
                InsertNonFull(_root, newValueToInsert);
                return;
            }

            //Если да, то создаём новый(пустой) корень, а старый записываем в ветки
            Node oldRoot = _root;
            _root = new Node(_size);
            _root.Leaves.Add(oldRoot);
            SplitChild(_root, 0, oldRoot);
            InsertNonFull(_root, newValueToInsert);
        }

        /// <summary>
        /// Метод для вставки в листья или корень(если листья заполнены)
        /// </summary>
        /// <param name="node">Нода для вставки</param>
        /// <param name="newValueToInsert">Значение</param>
        private void InsertNonFull(Node node, int newValueToInsert)
        {
            //Определяем позицию для вставки значения
            int positionToInsert = 0;
            for (int i = 0; i < node.Values.Count; i++)
            {
                if (newValueToInsert >= node.Values[i])
                {
                    positionToInsert++;
                }
            }

            //Если нода является листом, то сразу вставляем значение
            if (node.IsLeaf)
            {
                node.Values.Insert(positionToInsert, newValueToInsert);
                return;
            }

            //Если нет, то находим ветку, которая подходит под значение
            var child = node.Leaves[positionToInsert];
            //Если она заполнена, то разделяем эту ветку
            if (child.IsFull)
            {
                SplitChild(node, positionToInsert, child);
                if (newValueToInsert > node.Values[positionToInsert])
                {
                    positionToInsert++;
                }
            }

            InsertNonFull(node.Leaves[positionToInsert], newValueToInsert);
        }

        /// <summary>
        /// Метод для разделения ветки (листа) при переполнении
        /// </summary>
        /// <param name="parentNode">родительская нода</param>
        /// <param name="nodeToBeSplitIndex">индекс для разделения</param>
        /// <param name="nodeToBeSplit">нода для разделения</param>
        private void SplitChild(Node parentNode, int nodeToBeSplitIndex, Node nodeToBeSplit)
        {
            var newNode = new Node(_size);
            // Разделение на два листа(ветки) и вставка среднего значения в родительскую ноду
            parentNode.Values.Insert(nodeToBeSplitIndex, nodeToBeSplit.Values[_size - 1]);
            parentNode.Leaves.Insert(nodeToBeSplitIndex + 1, newNode);

            newNode.Values.AddRange(nodeToBeSplit.Values.GetRange(_size, _size - 1));
            // Убираем из старой ноды перенесённые значения
            nodeToBeSplit.Values.RemoveRange(_size - 1, _size);

            // Если разделяемая нода не является листом, то значения листьев(веток), так же перезаписываются в новую ноду
            if (!nodeToBeSplit.IsLeaf)
            {
                newNode.Leaves.AddRange(nodeToBeSplit.Leaves.GetRange(_size, _size));
                nodeToBeSplit.Leaves.RemoveRange(_size, _size);
            }
        }

        #endregion

        #region Delete
        /// <summary>
        /// Метод для удаления значения
        /// </summary>
        /// <param name="valueToDelete">Значение для удаления</param>
        public void Delete(int valueToDelete)
        {
            CoreDelete(_root, valueToDelete);

            //Eсли в корне нет значений, то удаляем все листья
            if (_root.Values.Count == 0 && !_root.IsLeaf)
            {
                _root = _root.Leaves.Single();
            }
        }

        /// <summary>
        /// Основной метод для удаления
        /// </summary>
        /// <param name="node">Нода для удаления</param>
        /// <param name="valueToDelete">Значение на удаление</param>
        private void CoreDelete(Node node, int valueToDelete)
        {
            int index = 0;
            // Находим позицию на удаление
            for (int i = 0; i < node.Values.Count; i++)
            {
                if (valueToDelete > node.Values[i])
                {
                    index++;
                }
            }

            // Если значение находится в ноде, то удаляем его
            if (index < node.Values.Count && node.Values[index] == valueToDelete)
            {
                DeleteValueFromNode(node, valueToDelete, index);
                return;
            }

            // Если нет - идём дальше
            if (!node.IsLeaf)
            {
                DeleteValueFromChild(node, valueToDelete, index);
            }
        }

        /// <summary>
        /// Удаление из дочернего узла(В нём просисходи ПОЛНОЕ перестроение дерева)
        /// </summary>
        /// <param name="parentNode">Родительский узел</param>
        /// <param name="valueToDelete">Значение на удаление</param>
        /// <param name="indexInLeaves">Индекс в списке листьев(веток)</param>
        private void DeleteValueFromChild(Node parentNode, int valueToDelete, int indexInLeaves)
        {
            // Находим дочерний узел
            Node childNode = parentNode.Leaves[indexInLeaves];

            // Если в нём минимально возможное количество значений, то начинаем удаление узла и перебалансировку дерева
            if (childNode.IsMinValues)
            {
                int leftIndex = indexInLeaves - 1;
                Node leftSibling = indexInLeaves > 0 ? parentNode.Leaves[leftIndex] : null;

                int rightIndex = indexInLeaves + 1;
                Node rightSibling = indexInLeaves < parentNode.Leaves.Count - 1
                                                ? parentNode.Leaves[rightIndex]
                                                : null;

                if (leftSibling != null && leftSibling.Values.Count > _size - 1)
                {
                    childNode.Values.Insert(0, parentNode.Values[indexInLeaves]);
                    parentNode.Values[indexInLeaves] = leftSibling.Values.Last();
                    leftSibling.Values.RemoveAt(leftSibling.Values.Count - 1);

                    if (!leftSibling.IsLeaf)
                    {
                        childNode.Leaves.Insert(0, leftSibling.Leaves.Last());
                        leftSibling.Leaves.RemoveAt(leftSibling.Leaves.Count - 1);
                    }
                }
                else if (rightSibling != null && rightSibling.Values.Count > _size - 1)
                {
                    childNode.Values.Add(parentNode.Values[indexInLeaves]);
                    parentNode.Values[indexInLeaves] = rightSibling.Values.First();
                    rightSibling.Values.RemoveAt(0);

                    if (!rightSibling.IsLeaf)
                    {
                        childNode.Leaves.Add(rightSibling.Leaves.First());
                        rightSibling.Leaves.RemoveAt(0);
                    }
                }
                else
                {
                    if (leftSibling != null)
                    {
                        var position = indexInLeaves - 1 >= 0 ? indexInLeaves - 1 : 0;
                        childNode.Values.Insert(0, parentNode.Values[position]);
                        var oldValues = childNode.Values;
                        childNode.Values = leftSibling.Values;
                        childNode.Values.AddRange(oldValues);
                        if (!leftSibling.IsLeaf)
                        {
                            var oldLeaves = childNode.Leaves;
                            childNode.Leaves = leftSibling.Leaves;
                            childNode.Leaves.AddRange(oldLeaves);
                        }

                        parentNode.Leaves.RemoveAt(leftIndex);
                        parentNode.Values.RemoveAt(position);
                    }
                    else
                    {
                        childNode.Values.Add(parentNode.Values[indexInLeaves]);
                        childNode.Values.AddRange(rightSibling.Values);
                        if (!rightSibling.IsLeaf)
                        {
                            childNode.Leaves.AddRange(rightSibling.Leaves);
                        }

                        parentNode.Leaves.RemoveAt(rightIndex);
                        parentNode.Values.RemoveAt(indexInLeaves);
                    }
                }
            }

            CoreDelete(childNode, valueToDelete);
        }

        /// <summary>
        /// Метод для удаления значения в узле
        /// </summary>
        /// <param name="node">Сам узел</param>
        /// <param name="valueToDelete">Значение на удаление</param>
        /// <param name="keyIndexInNode">Индекс для удаления</param>
        private void DeleteValueFromNode(Node node, int valueToDelete, int keyIndexInNode)
        {
            // Если узел является листом, то просто удаляем
            if (node.IsLeaf)
            {
                node.Values.RemoveAt(keyIndexInNode);
                return;
            }

            // Если значение в ветке - перестраиваем часть дерева
            Node leftNode = node.Leaves[keyIndexInNode];
            if (leftNode.Values.Count >= _size)
            {
                // Полученное значение вставляем в текущий узел
                int predecessor = DeleteRightChild(leftNode);
                node.Values[keyIndexInNode] = predecessor;
            }
            else
            {
                // Получаем правый узел от значения и у него проверяем кол-во элементов
                Node rightNode = node.Leaves[keyIndexInNode + 1];
                if (rightNode.Values.Count >= _size)
                {
                    // Полученное значение вставляем в текущий узел
                    int successor = DeleteLeftChild(leftNode);
                    node.Values[keyIndexInNode] = successor;
                }
                else
                {
                    // Если ни одно из услови выше не подошло, то ребалансим листья(ветки)
                    leftNode.Values.Add(node.Values[keyIndexInNode]);
                    leftNode.Values.AddRange(rightNode.Values);
                    leftNode.Leaves.AddRange(rightNode.Leaves);

                    node.Values.RemoveAt(keyIndexInNode);
                    node.Leaves.RemoveAt(keyIndexInNode + 1);

                    CoreDelete(leftNode, valueToDelete);
                }
            }
        }

        /// <summary>
        /// Метод, удаляющий последнее значение узла
        /// </summary>
        /// <param name="node">Узел для удаления</param>
        /// <returns>Удалённое значение</returns>
        private int DeleteRightChild(Node node)
        {
            // Если узел является листом, удаляем последний элемент и возвращаем его
            if (node.IsLeaf)
            {
                var result = node.Values[^1];
                node.Values.RemoveAt(node.Values.Count - 1);
                return result;
            }

            // Если узел не является листом, то удаляем последний элемент поледнего дочернего узла
            return DeleteLeftChild(node.Leaves.Last());
        }

        /// <summary>
        /// Метод, удаляющий первое значение узла
        /// </summary>
        /// <param name="node">Узел для удаления</param>
        /// <returns>Удалённое значение</returns>
        private int DeleteLeftChild(Node node)
        {
            // Если узел является листом, удаляем первый элемент и возвращаем его
            if (node.IsLeaf)
            {
                var result = node.Values[0];
                node.Values.RemoveAt(0);
                return result;
            }

            // Если узел не является листом, то удаляем последний элемент первого дочернего узла
            return DeleteRightChild(node.Leaves.First());
        }

        #endregion

        private int degree = 0;

        /// <summary>
        /// Переопределённый метод для вывода дерева в консоль
        /// </summary>
        /// <returns>Строковое значение</returns>
        public override string ToString()
        {
            var pairs = new List<(int, int[])>();

            var strings = new List<string>();
            GetAllLevels(_root, pairs, 0);
            for (int i = degree; i >= 0; i--)
            {
                strings.Insert(0, GetStringForCurrentLevel(pairs, degree - i));
            }

            for (int i = 0; i < strings.Count; i++)
            {
                Console.WriteLine(strings[i]);
                Console.WriteLine();
            }

            return string.Empty;
        }

        /// <summary>
        /// Метод для создания строк для каждого уровня
        /// </summary>
        /// <param name="pairs">Записанные пары</param>
        /// <param name="tabsCount">Количество табов</param>
        /// <returns>Строку для уровня</returns>
        private string GetStringForCurrentLevel(List<(int, int[])> pairs, int tabsCount)
        {
            var list = pairs.Where(x => x.Item1 == degree - tabsCount).Select(x => x.Item2).ToList();

            string output = string.Empty;
            string tabs = tabsCount == 0 ? "  ": new string('\t', tabsCount);
            foreach (var item in list)
            {
                string values = string.Empty;
                for (int i = 0; i < item.Length; i++)
                {
                    values += $" {item[i]} ";
                }

                output += $"{tabs} {values} {tabs}";
            }

            return output;
        }

        /// <summary>
        /// Метод, записывающий все пары порядок - значения в динамический список
        /// </summary>
        /// <param name="node">Нода дял старта</param>
        /// <param name="pairs">Список пар</param>
        /// <param name="position">Позиция(порядок) для чтения</param>
        private void GetAllLevels(Node node, List<(int,int[])> pairs, int position)
        {
            pairs.Add((position, node.Values.ToArray()));

            if (degree < position)
            {
                degree = position;
            }

            foreach (var leaf in node.Leaves)
            {
                GetAllLevels(leaf, pairs, position + 1);
            }
        }
    }
}
