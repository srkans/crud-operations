using Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents data access logic for managing Person entity
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Adds a person object to the data store
        /// </summary>
        /// <param name="person">Person object to add</param>
        /// <returns>returns the person object after adding it to the table</returns>
        Task<Person> AddPerson(Person person);

        /// <summary>
        /// Returns all persons in the data store
        /// </summary>
        /// <returns>List of person object from table</returns>
        Task<List<Person>> GetAllPersons();

        /// <summary>
        /// returns a person object based on the given person id
        /// </summary>
        /// <param name="personID">personID(guid) to search</param>
        /// <returns>A person object or null</returns>
        Task<Person?> GetPersonByPersonID(Guid personID);

        /// <summary>
        /// Returns all person object based on the given expression
        /// </summary>
        /// <param name="predicate">LINQ expression to check</param>
        /// <returns>All matching persons with given condition</returns>
        Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate); //it will get lambda expression as an argument
                                                                                         //parameter->person return->bool
        /// <summary>
        /// Deletes an object based on the person Id
        /// </summary>
        /// <param name="personID">Person id(guid) to search</param>
        /// <returns>Returns true if the deletion is successful, otherwise false</returns>
        Task<bool> DeletePersonByPersonID(Guid personID);

        /// <summary>
        /// Updates a person object based on the given id
        /// </summary>
        /// <param name="person">Person object to update</param>
        /// <returns>Returns the updated person object</returns>
        Task<Person> UpdatePerson(Person person);
    }

}
