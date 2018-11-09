CREATE TABLE `order2` (
  `CurrentState` varchar(36) DEFAULT NULL,
  `Created` datetime DEFAULT NULL,
  `Updated` datetime DEFAULT NULL,
  `CustomerName` varchar(36) DEFAULT NULL,
  `CustomerPhone` varchar(36) DEFAULT NULL,
  `RejectedReasonPhrase` varchar(36) DEFAULT NULL,
  `CorrelationId` varchar(36) DEFAULT NULL,
  `EstimatedTime` int(11) DEFAULT NULL,
  `OrderID` int(11) DEFAULT NULL,
  `Status` int(11) DEFAULT NULL,
  `PizzaID` int(11) NOT NULL,
  `DomainOperationRequestId` char(16)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;