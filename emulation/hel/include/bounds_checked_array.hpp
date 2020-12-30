#ifndef _BOUNDS_CHECKED_ARRAY_HPP_
#define _BOUNDS_CHECKED_ARRAY_HPP_

#include "error.hpp"

#include <array>
#include <functional>
#include <stdexcept>

namespace hel{

    /**
     * \brief Array wrapper with bounds checking and helpful operators
     * \tparam T The type the BoundsCheckedArray stores
     * \tparam LEN The length of the BoundsCheckedArray
     */
    template<typename T, std::size_t LEN>
    struct BoundsCheckedArray{
        /**
         * \brief Define value_type for consistency among C++ iterable containers
         */

        using value_type = T;

    private:

        /**
         * \brief The internal data array for this wrapper
         */

        std::array<T, LEN> internal;

    public:

        /**
         * \brief Returns a reference to the element at pos with bounds checking
         * \param pos The position of the element to return
         * \return A reference to the requested element
         */

        constexpr const T& at(std::size_t pos)const{
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN)));
            }
            return internal[pos];
        }

        /**
         * \brief Returns a reference to the element at pos with bounds checking
         * \param pos The position of the element to return
         * \return A reference to the requested element
         */

        constexpr T& at(std::size_t pos){
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN)));
            }
            return internal[pos];
        }

        /**
         * \brief Returns a reference to the element at pos with bounds checking
         * \param pos The position of the element to return
         * \return A reference to the requested element
         */

        constexpr const T& operator[](std::size_t pos)const{
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN)));
            }
            return internal[pos];
        }

        /**
         * \brief Returns a reference to the element at pos with bounds checking
         * \param pos The position of the element to return
         * \return A reference to the requested element
         */

        constexpr T& operator[](std::size_t pos){
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN)));
            }
            return internal[pos];
        }

        /**
         * \brief Returns a reference to the first element
         * \return A reference to the first element
         */

        constexpr const T& front()const{
            if(LEN == 0){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: cannot reference front of array of size " + std::to_string(LEN)));
            }
            return internal.front();
        }

        /**
         * \brief Returns a reference to the first element
         * \return A reference to the first element
         */

        constexpr T& front(){
            if(LEN == 0){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: cannot reference front of array of size " + std::to_string(LEN)));
            }
            return internal.front();
        }

        /**
         * \brief Returns a reference to the last element
         * \return A reference to the last element
         */

        constexpr const T& back()const{
            if(LEN == 0){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: cannot reference back of array of size " + std::to_string(LEN)));
            }
            return internal.back();
        }

        /**
         * \brief Returns a reference to the last element
         * \return A reference to the last element
         */

        constexpr T& back(){
            if(LEN == 0){
                throw std::out_of_range(makeExceptionMessage("array index out of bounds: cannot reference back of array of size " + std::to_string(LEN)));
            }
            return internal.back();
        }

        /**
         * \brief Fetches the internal std::array of the BoundsCheckedArray
         * \return The internal std::array of this BoundsCheckedArray
         */

        constexpr const std::array<T, LEN>& toArray()const noexcept{
            return internal;
        }

        /**
         * \brief Fetches the internal std::array of the BoundsCheckedArray
         * \return The internal std::array of this BoundsCheckedArray
         */

        constexpr std::array<T, LEN>& toArray()noexcept{
            return internal;
        }

        /**
         * \brief Fetches the underlying basic array
         * \return A pointer to the underlying basic array
         */

        constexpr const T* data()const noexcept{
            if(LEN == 0){
                return nullptr;
            }
            return internal.data();
        }

        /**
         * \brief Fetches the underlying basic array
         * \return A pointer to the underlying basic array
         */

        constexpr T* data()noexcept{
            if(LEN == 0){
                return nullptr;
            }
            return internal.data();
        }

        /**
         * \brief Fetches the number of elements in the array
         * \return The number of elements in the array
         */

        constexpr std::size_t size()const noexcept{
            return internal.size();
        }

        /**
         * \brief Checks if the container has no elements
         * \return true if the array is empty, false otherwise
         */

        constexpr bool empty()const noexcept{
            return internal.empty();
        }

        /**
         * \brief Returns the maximum number of elements the array can hold
         * \return Maximum number of elements
         */

        constexpr std::size_t max_size()const noexcept{
            return internal.max_size();
        }

        /**
         * \brief Returns an iterator to the first element in the container
         * \return Iterator to the first element
         */

        constexpr typename std::array<T, LEN>::iterator begin()noexcept{
            return internal.begin();
        }

        /**
         * \brief Returns an iterator to the first element in the container
         * \return Iterator to the first element
         */

        constexpr typename std::array<T, LEN>::const_iterator begin()const noexcept{
            return internal.begin();
        }

        /**
         * \brief Returns an iterator to the first element in the container
         * \return Iterator to the first element
         */

        constexpr typename std::array<T, LEN>::const_iterator cbegin()const noexcept{
            return internal.cbegin();
        }

        /**
         * \brief Returns an iterator to the last element in the container
         * \return Iterator to the last element
         */

        constexpr typename std::array<T, LEN>::iterator end()noexcept{
            return internal.end();
        }

        /**
         * \brief Returns an iterator to the last element in the container
         * \return Iterator to the last element
         */

        constexpr typename std::array<T, LEN>::const_iterator end()const noexcept{
            return internal.end();
        }
        /**
         * \brief Returns an iterator to the last element in the container
         * \return Iterator to the last element
         */

        constexpr typename std::array<T, LEN>::const_iterator cend()const noexcept{
            return internal.cend();
        }

        /**
         * Constructor for a BoundsCheckedArray
         *
         * \param default_data A default value to fill the array with
         */

        BoundsCheckedArray(const T& default_data)noexcept{
            internal.fill(default_data);
        }

        /**
         * Constructor for a BoundsCheckedArray
         *
         * \param iterable An iterable object to copy
         */

        template<typename S, typename = std::enable_if<std::is_same<typename S::value_type,T>::value && !std::is_same<S,std::initializer_list<T>>::value>>
        BoundsCheckedArray(const S& iterable)noexcept{
            if(iterable.size() != LEN){
                throw std::out_of_range(makeExceptionMessage("assignment to array of size " + std::to_string(LEN) + " to iterable of different size " + std::to_string(iterable.size())));
            }
            std::copy(iterable.begin(), iterable.end(), internal.begin());

        }

        /**
         * Constructor for a BoundsCheckedArray
         *
         * \param list An initializer list to copy
         */

        template<typename S, typename = std::enable_if<!std::is_same<S,T>::value>> //TODO template necessary?
        BoundsCheckedArray(std::initializer_list<T> list){
            if(list.size() != LEN){
                throw std::out_of_range(makeExceptionMessage("assignment to array of size " + std::to_string(LEN) + " to brace-enclosed initializer list of different size " + std::to_string(list.size())));
            }
            std::copy(list.begin(), list.end(), internal.begin());
        }

        /**
         * Deconstructor for a BoundsCheckedArray
         */

        ~BoundsCheckedArray() = default;

        template<typename S, std::size_t L>
        friend bool operator==(const BoundsCheckedArray<S, L>&, const BoundsCheckedArray<S, L>&);

        template<typename S, std::size_t L>
        friend bool operator!=(const BoundsCheckedArray<S, L>&, const BoundsCheckedArray<S, L>&);
    };

    /**
     * \brief Equality comparison operator for two BoundsCheckedArray objects
     * \param a The first object to compare against
     * \param b The second object to compare against
     * \return True if the two BoundsCheckedArray objects are equal
     */

    template<typename T, std::size_t LEN>
    bool operator==(const BoundsCheckedArray<T, LEN>& a, const BoundsCheckedArray<T, LEN>& b){
        return a.internal == b.internal;
    }

    /**
     * \brief Inequality comparison operator for two BoundsCheckedArray objects
     * \param a The first object to compare against
     * \param b The second object to compare against
     * \return True if the two BoundsCheckedArray objects are not equal
     */

    template<typename T, std::size_t LEN>
    bool operator!=(const BoundsCheckedArray<T, LEN>& a, const BoundsCheckedArray<T, LEN>& b){
        return !(a == b);
    }
}

#endif
